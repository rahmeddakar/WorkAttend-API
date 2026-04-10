using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using WorkAttend.API.Gateway.DAL.services.SubscriptionServices;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.Common.Helper
{
    public class SubscriptionPaymentHelper
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public SubscriptionPaymentHelper(
            ISubscriptionService subscriptionService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _subscriptionService = subscriptionService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<SiteTokenModel> TopupAsync(int companyId, int userId, int subscriptionId, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    return new SiteTokenModel
                    {
                        data = "Please provide valid amount to topup.",
                        msgCode = "500",
                        msgDescription = "Invalid Amount"
                    };
                }

                string guid = Guid.NewGuid().ToString();
                int startIndex = guid.LastIndexOf("-");
                string sessionId = guid.Substring(startIndex + 1);

                var transactionLog = new TransactionLog
                {
                    ReferenceCode = sessionId,
                    CompanyId = companyId,
                    UserId = userId,
                    SubscriptionID = subscriptionId,
                    TopupAmount = amount,
                    IsTopUpTransaction = true,
                    RequestJson = JsonSerializer.Serialize(new
                    {
                        SubscriptionId = subscriptionId,
                        Amount = amount
                    }),
                    IsCompleted = false,
                    CreatedOn = DateTime.Now
                };

                transactionLog = await _subscriptionService.SaveOrUpdateTransactionLogAsync(transactionLog);

                string baseUrl = _configuration["Payment:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    baseUrl = "https://payment.workattend.com/";

                baseUrl = baseUrl.TrimEnd('/') + "/";

                string redirectUrl =
                    $"{baseUrl}subscription/ConfirmPaymentRedirect?token={WebUtility.UrlEncode(sessionId)}&userID={WebUtility.UrlEncode(userId.ToString())}";

                string statusUrl =
                    $"{baseUrl}subscription/ConfirmPaymentStatus?token={WebUtility.UrlEncode(sessionId)}&userID={WebUtility.UrlEncode(userId.ToString())}";

                APCOTransaction transaction = new APCOTransaction
                {
                    ActionType = 1,
                    RedirectionURL = redirectUrl,
                    status_url = statusUrl,
                    FailedRedirectionURL = redirectUrl,
                    ORef = sessionId,
                    Value = amount,
                    Email = string.Empty,
                    CSSTemplate = "Default"
                };

                XElement root = new XElement("Transaction");
                root.SetAttributeValue("hash", PaymentGatewayConfigs.MerchantSecretWord);

                foreach (var prop in transaction.GetType().GetProperties())
                {
                    string val = prop.GetValue(transaction) != null ? prop.GetValue(transaction).ToString() : string.Empty;

                    if (prop.Name != "PspID")
                    {
                        if (prop.Name == "status_url")
                        {
                            XElement statusElement = new XElement(prop.Name, !string.IsNullOrEmpty(val) ? val : null);
                            statusElement.SetAttributeValue("urlEncode", "true");
                            root.Add(statusElement);
                        }
                        else
                        {
                            root.Add(new XElement(prop.Name, !string.IsNullOrEmpty(val) ? val : null));
                        }
                    }
                }

                string transactionXml = root.ToString();

                APCOBuildTokenRequest buildToken = new APCOBuildTokenRequest(transactionXml);

                XElement tokenXmlRoot = new XElement("BuildXMLToken");
                tokenXmlRoot.SetAttributeValue("XMLTAGTOREPLACE", "XMLURLTOREPLACE");

                foreach (var prop in buildToken.GetType().GetProperties())
                {
                    string val = prop.GetValue(buildToken) != null ? prop.GetValue(buildToken).ToString() : string.Empty;
                    tokenXmlRoot.Add(new XElement(prop.Name, !string.IsNullOrEmpty(val) ? val : null));
                }

                string buildTokenXml = tokenXmlRoot.ToString()
                    .Replace("XMLTAGTOREPLACE", "xmlns")
                    .Replace("XMLURLTOREPLACE", PaymentGatewayConfigs.MerchantDomain);

                string responseJson = await CallSoapWebServiceAsync(PaymentGatewayConfigs.MerchantAPIEndPoint, buildTokenXml);

                if (!string.IsNullOrWhiteSpace(responseJson))
                {
                    XDocument buildTokenXmlDocument = XDocument.Parse(responseJson);

                    var buildTokenSoapResponse = buildTokenXmlDocument
                        .Descendants()
                        .FirstOrDefault(x => x.Name.LocalName == "BuildXMLTokenResult");

                    if (buildTokenSoapResponse != null && !string.IsNullOrWhiteSpace(buildTokenSoapResponse.Value))
                    {
                        string paymentUrl = $"{PaymentGatewayConfigs.MerchantCheckOutUrl}{buildTokenSoapResponse.Value}";

                        transactionLog.APCOURL = paymentUrl;
                        await _subscriptionService.SaveOrUpdateTransactionLogAsync(transactionLog);

                        return new SiteTokenModel
                        {
                            data = new { isok = 1, url = paymentUrl },
                            msgCode = "200",
                            msgDescription = "Payment Request Successful."
                        };
                    }

                    return new SiteTokenModel
                    {
                        data = new { isok = 0, url = string.Empty },
                        msgCode = "500",
                        msgDescription = "Payment Response Error."
                    };
                }

                return new SiteTokenModel
                {
                    data = new { isok = 0, url = string.Empty },
                    msgCode = "500",
                    msgDescription = "Payment Request Creation Error."
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "SubscriptionPaymentHelper.TopupAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new SiteTokenModel
                {
                    data = "Internal Server Error.",
                    msgCode = "500",
                    msgDescription = ex.ToString()
                };
            }
        }

        public async Task<string> ConfirmPaymentStatusAsync(string userId, string token)
        {
            var result = await GetPaymentResponseAsync(userId, token);
            return result.Response ?? string.Empty;
        }

        public async Task<string> GetPaymentRedirectUrlAsync(string userId, string token)
        {
            var result = await GetPaymentResponseAsync(userId, token);

            if (result.Model.IsOk)
            {
                string amount = string.Format("{0:0.00}", result.Model.Amount);
                return $"/subscription/Index#{amount}#{result.Model.Message}";
            }

            return $"/subscription/Index#0#{result.Model.Message}";
        }

        private async Task<(PaymentResponseModel Model, string Response)> GetPaymentResponseAsync(string userId, string token)
        {
            string response = string.Empty;
            PaymentResponseModel model = new PaymentResponseModel
            {
                IsOk = false,
                Amount = 0.0M,
                Message = "No transaction found."
            };

            int parsedUserId = 0;
            int.TryParse(userId, out parsedUserId);

            var transaction = await _subscriptionService.GetTransactionLogAsync(userId, token);
            if (transaction == null)
            {
                response = "NOTOK";
                return (model, response);
            }

            if (!transaction.IsCompleted)
            {
                try
                {
                    XmlDocument objFastPayXml = GetParamsAndConvertToXmlDocument(out string param);

                    string resultValue = string.Empty;
                    string oRef = token;

                    if (objFastPayXml != null)
                    {
                        var resultElement = objFastPayXml.GetElementsByTagName("Result");
                        resultValue = resultElement != null && resultElement.Count > 0
                            ? resultElement.Item(0).InnerText
                            : string.Empty;

                        var oRefElement = objFastPayXml.GetElementsByTagName("ORef");
                        oRef = oRefElement != null && oRefElement.Count > 0
                            ? oRefElement.Item(0).InnerText
                            : token;
                    }

                    response = NormalizePaymentResponse(resultValue);

                    APCOTransactionStatusByORefRequest transactionStatusRequest = new APCOTransactionStatusByORefRequest
                    {
                        Oref = oRef
                    };

                    XElement root = new XElement("getTransactionsByORef");
                    root.SetAttributeValue("XMLTAGTOREPLACE", "XMLURLTOREPLACE");

                    foreach (var prop in transactionStatusRequest.GetType().GetProperties())
                    {
                        string val = prop.GetValue(transactionStatusRequest) != null ? prop.GetValue(transactionStatusRequest).ToString() : string.Empty;
                        root.Add(new XElement(prop.Name, !string.IsNullOrEmpty(val) ? val : null));
                    }

                    string finalXml = root.ToString()
                        .Replace("XMLTAGTOREPLACE", "xmlns")
                        .Replace("XMLURLTOREPLACE", PaymentGatewayConfigs.MerchantDomain);

                    string responseJson = await CallSoapWebServiceAsync(PaymentGatewayConfigs.MerchantAPIEndPoint, finalXml);

                    if (string.IsNullOrWhiteSpace(responseJson))
                    {
                        model.Message = "Payment response error from merchant gateway.";
                        response = "NOTOK";
                        return (model, response);
                    }

                    bool isValid = false;
                    bool isPaid = false;
                    decimal paymentAmount = 0.0M;
                    string paymentAmountCurrency = string.Empty;
                    string paymentCustomerId = string.Empty;
                    string paymentPspId = string.Empty;
                    string paymentResponseJson = string.Empty;
                    string cardNum = string.Empty;
                    string cardHolderName = string.Empty;
                    string cardType = string.Empty;
                    string cardCountryBin = string.Empty;
                    string paymentORef = oRef;

                    XDocument xml = XDocument.Parse(responseJson);

                    var responseNode = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Response");
                    if (responseNode != null && !string.IsNullOrWhiteSpace(responseNode.Value))
                    {
                        string statusResponse = WebUtility.UrlDecode(responseNode.Value);

                        APCOTransactionStatusModel apcoResult = null;
                        var serializer = new XmlSerializer(typeof(APCOTransactionStatusModel));

                        using (var sr = new StringReader(statusResponse))
                        {
                            try
                            {
                                apcoResult = (APCOTransactionStatusModel)serializer.Deserialize(sr);
                            }
                            catch
                            {
                                apcoResult = null;
                            }
                        }

                        if (apcoResult != null)
                        {
                            isValid = true;
                            paymentResponseJson = JsonSerializer.Serialize(apcoResult);
                            paymentORef = apcoResult.ORef;
                            paymentAmount = apcoResult.Value;
                            paymentAmountCurrency = apcoResult.Currency;
                            paymentCustomerId = apcoResult.Email;
                            paymentPspId = apcoResult.pspid;
                            cardNum = apcoResult.CardInput;
                            isPaid = !string.IsNullOrEmpty(apcoResult.accepted)
                                ? apcoResult.accepted == "YES"
                                : apcoResult.AuthCode == "CAPTURED";
                        }
                    }
                    else
                    {
                        var tableNode = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Table1");
                        if (tableNode != null)
                        {
                            APCO3DSecureTransactionResponse secureTransactionResult = null;
                            var serializer = new XmlSerializer(typeof(APCO3DSecureTransactionResponse));

                            using (var sr = new StringReader(tableNode.ToString()))
                            {
                                try
                                {
                                    secureTransactionResult = (APCO3DSecureTransactionResponse)serializer.Deserialize(sr);
                                }
                                catch
                                {
                                    secureTransactionResult = null;
                                }
                            }

                            if (secureTransactionResult != null)
                            {
                                isValid = true;
                                paymentResponseJson = JsonSerializer.Serialize(secureTransactionResult);
                                paymentORef = secureTransactionResult.OrderRef;
                                paymentAmount = secureTransactionResult.Amount;
                                paymentAmountCurrency = secureTransactionResult.CurrencyCode;
                                paymentCustomerId = secureTransactionResult.Email;
                                paymentPspId = secureTransactionResult.PSPID;
                                cardNum = secureTransactionResult.CardNum;
                                cardCountryBin = secureTransactionResult.CountryBIN;
                                cardHolderName = secureTransactionResult.CardHname;
                                cardType = secureTransactionResult.CardType;
                                isPaid = secureTransactionResult.BankAccept?.ToUpper() == "YES"
                                         && secureTransactionResult.BankResponse?.ToUpper() == "CAPTURED";
                            }
                        }
                    }

                    if (!isValid)
                    {
                        model.Message = "Payment response error from merchant gateway.";
                        response = "NOTOK";
                        return (model, response);
                    }

                    PaymentTransaction paymentTransaction = new PaymentTransaction
                    {
                        ReferenceCode = transaction.ReferenceCode,
                        GatewayResponse = param,
                        PaymentTokenId = paymentORef,
                        PaymentAmount = paymentAmount,
                        PaymentAmountCurrency = paymentAmountCurrency,
                        PaymentCustomerId = paymentCustomerId,
                        PaymentPspID = paymentPspId,
                        PaymentProcessDate = DateTime.Now,
                        IsPaid = isPaid,
                        CountryBIN = cardCountryBin,
                        CardNum = cardNum,
                        CardHolderName = cardHolderName,
                        CardType = cardType,
                        CreatedOn = DateTime.Now,
                        PaymentResponse = paymentResponseJson
                    };

                    var transactionRecheck = await _subscriptionService.GetTransactionLogAsync(userId, token);
                    if (transactionRecheck != null && !transactionRecheck.IsCompleted)
                    {
                        var newPaymentCompanyTransaction = await _subscriptionService.SavePaymentCompanyTransactionAsync(paymentTransaction);

                        transaction.IsCompleted = true;
                        transaction.CompletedOn = DateTime.Now;
                        transaction.PaymentTransactionId = newPaymentCompanyTransaction.PaymentTransactionId;

                        await _subscriptionService.SaveOrUpdateTransactionLogAsync(transaction);
                    }

                    model.Amount = paymentAmount;
                    model.IsOk = paymentTransaction.IsPaid;
                    model.Message = paymentTransaction.IsPaid
                        ? "Balance Topup Successfully."
                        : "Balance topup not successful.";

                    response = paymentTransaction.IsPaid ? "OK" : "NOTOK";
                    return (model, response);
                }
                catch (Exception ex)
                {
                    await LoggingHelper.InsertException(
                        ex.Source ?? string.Empty,
                        ex.Message,
                        "SubscriptionPaymentHelper.GetPaymentResponseAsync",
                        ex.StackTrace ?? string.Empty,
                        ex.InnerException?.ToString() ?? string.Empty);

                    model.Amount = 0.0M;
                    model.IsOk = false;
                    model.Message = "Internal Server Error.";
                    response = "NOTOK";
                    return (model, response);
                }
            }
            else
            {
                bool isPaid = false;

                if (transaction.PaymentTransactionId.HasValue && transaction.PaymentTransactionId.Value > 0)
                {
                    var transactionRecheck = await _subscriptionService.GetPaymentTransactionAsync(transaction.PaymentTransactionId.Value);
                    if (transactionRecheck != null)
                    {
                        isPaid = transactionRecheck.IsPaid;
                    }
                }

                model.Amount = transaction.TopupAmount;
                model.IsOk = isPaid;
                model.Message = isPaid ? "Balance Topup Successfully." : "Balance topup not successful.";
                response = isPaid ? "OK" : "NOTOK";

                return (model, response);
            }
        }

        private XmlDocument GetParamsAndConvertToXmlDocument(out string param)
        {
            param = string.Empty;
            XmlDocument objParamsXml = new XmlDocument();

            try
            {
                string strParamsVal = string.Empty;

                param = _httpContextAccessor.HttpContext?.Request?.Query["params"].ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(param))
                {
                    strParamsVal = WebUtility.UrlDecode(param);
                }
                else
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(strParamsVal))
                {
                    objParamsXml.LoadXml(strParamsVal);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            return objParamsXml;
        }

        private async Task<string> CallSoapWebServiceAsync(string baseUrl, string xmlMessage)
        {
            using var client = new HttpClient();

            string requestXml =
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
   {xmlMessage}
  </soap:Body>
</soap:Envelope>";

            using var content = new StringContent(requestXml, Encoding.UTF8, "text/xml");
            using var response = await client.PostAsync(baseUrl, content);

            return await response.Content.ReadAsStringAsync();
        }

        private static string NormalizePaymentResponse(string resultValue)
        {
            if (string.IsNullOrWhiteSpace(resultValue))
                return string.Empty;

            switch (resultValue.ToUpper().Trim())
            {
                case "OK":
                    return "OK";
                case "NOTOK":
                    return "NOTOK";
                case "DECLINED":
                    return "DECLINED";
                case "PENDING":
                    return "PENDING";
                case "CANCEL":
                    return "CANCEL";
                default:
                    return string.Empty;
            }
        }
    }
}