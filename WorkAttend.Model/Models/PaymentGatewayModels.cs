using System;
using System.Xml.Serialization;

namespace WorkAttend.Model.Models
{
    public class SiteTokenModel
    {
        public object data { get; set; }
        public string msgCode { get; set; }
        public string msgDescription { get; set; }
        public string msgType { get; set; }
    }

    public class PaymentTopupModel
    {
        public int SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public string baseURL { get; set; }
    }

    public class PaymentResponseModel
    {
        public string Message { get; set; }
        public bool IsOk { get; set; }
        public decimal Amount { get; set; }
    }

    public static class PaymentGatewayConfigs
    {
        public static string MerchantAPIEndPoint = "https://www.apsp.biz:9085/merchanttools.asmx";
        public static string MerchantCheckOutUrl = "https://www.apsp.biz/pay/FP6/Checkout.aspx?FPToken=";
        public static string MerchantCode = "3079";
        public static int MerchantCurrency = 978;
        public static string MerchantDomain = "https://www.apsp.biz/";
        public static string MerchantPassword = "uW4Gq7sG8VmJln";
        public static string MerchantProfileId = "290C4ED3E78A48129867AC3963017531";
        public static string MerchantSecretWord = "0c1ee50166";
    }

    public class APCOPaymentBase
    {
        public string MerchID => PaymentGatewayConfigs.MerchantCode;
        public string MerchPass => PaymentGatewayConfigs.MerchantPassword;
    }

    public class APCOTransaction
    {
        public string RedirectionURL { get; set; }
        public string status_url { get; set; }
        public string FailedRedirectionURL { get; set; }
        public string ProfileID => PaymentGatewayConfigs.MerchantProfileId;
        public int ActionType { get; set; }
        public decimal Value { get; set; }
        public int Curr => PaymentGatewayConfigs.MerchantCurrency;
        public string Lang => "en";
        public string ORef { get; set; }
        public string PspID { get; set; }
        public int? ClientAcc { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string RegCountry { get; set; }
        public string UDF1 { get; set; }
        public string UDF2 { get; set; }
        public string UDF3 { get; set; }
        public string CSSTemplate { get; set; }
        public string showDesc { get; set; }
        public string return_pspid { get; set; }
    }

    public class APCOBuildTokenRequest : APCOPaymentBase
    {
        private readonly string _encodedXmlRequest;

        public APCOBuildTokenRequest(string encodedXml)
        {
            _encodedXmlRequest = encodedXml;
        }

        public string XMLParam => _encodedXmlRequest;
    }

    [Serializable, XmlRoot(ElementName = "BuildXMLTokenResponse")]
    [XmlType("BuildXMLTokenResponse")]
    public class APCOBuildTokenResponse
    {
        [XmlElement(ElementName = "BuildXMLTokenResult")]
        public string BuildXMLTokenResult { get; set; }
    }

    public class APCOTransactionStatusByORefRequest
    {
        public string MCHCode => PaymentGatewayConfigs.MerchantCode;
        public string MCHPass => PaymentGatewayConfigs.MerchantPassword;
        public string Oref { get; set; }
    }

    public class APCOTransactionStatusResponse
    {
        public string Response { get; set; }
    }

    [Serializable, XmlRoot(ElementName = "TransactionResult")]
    [XmlType("TransactionResult")]
    public class APCOTransactionStatusModel
    {
        [XmlElement(ElementName = "ORef")]
        public string ORef { get; set; }

        [XmlElement(ElementName = "Result")]
        public string Result { get; set; }

        [XmlElement(ElementName = "AuthCode")]
        public string AuthCode { get; set; }

        [XmlElement(ElementName = "CardInput")]
        public string CardInput { get; set; }

        [XmlElement(ElementName = "pspid")]
        public string pspid { get; set; }

        [XmlElement(ElementName = "accepted")]
        public string accepted { get; set; }

        [XmlElement(ElementName = "Currency")]
        public string Currency { get; set; }

        [XmlElement(ElementName = "Value")]
        public decimal Value { get; set; }

        [XmlElement(ElementName = "Email")]
        public string Email { get; set; }
    }

    [Serializable, XmlRoot(ElementName = "Table1")]
    [XmlType("Table1")]
    public class APCO3DSecureTransactionResponse
    {
        [XmlElement(ElementName = "PSPID")]
        public string PSPID { get; set; }

        [XmlElement(ElementName = "CardNum")]
        public string CardNum { get; set; }

        [XmlElement(ElementName = "CardHname")]
        public string CardHname { get; set; }

        [XmlElement(ElementName = "CardType")]
        public string CardType { get; set; }

        [XmlElement(ElementName = "CurrencyCode")]
        public string CurrencyCode { get; set; }

        [XmlElement(ElementName = "Amount")]
        public decimal Amount { get; set; }

        [XmlElement(ElementName = "BankAccept")]
        public string BankAccept { get; set; }

        [XmlElement(ElementName = "BankResponse")]
        public string BankResponse { get; set; }

        [XmlElement(ElementName = "AuthCode")]
        public string AuthCode { get; set; }

        [XmlElement(ElementName = "CountryBIN")]
        public string CountryBIN { get; set; }

        [XmlElement(ElementName = "OrderRef")]
        public string OrderRef { get; set; }

        [XmlElement(ElementName = "Email")]
        public string Email { get; set; }
    }
}