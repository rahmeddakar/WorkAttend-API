//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Configuration;
//using System.Xml.Serialization;

//namespace WorkAttend.Model.Models
//{
//    /// <summary>
//    /// TODO: get these values from goabal config table
//    public static class GlobalAppConfigs
//    {
//        public static string MerchantAPIEndPoint = "https://www.apsp.biz:9085/merchanttools.asmx";
//        public static string MerchantBgColor = "#9CB2BE";
//        public static string MerchantCheckOutUrl = "https://www.apsp.biz/pay/FP6/Checkout.aspx?FPToken=";//"https://www.apsp.biz/pay/FP6/Checkout.aspx?FPToken=";
//        public static string MerchantCode = "3079";//"3519";
//        public static int    MerchantCurrency = 978;
//        public static string MerchantDomain = "https://www.apsp.biz/";
//        public static string MerchantPassword = "uW4Gq7sG8VmJln";//"qF8ew6IisLm5Ne";
//        public static string MerchantProfileId = "290C4ED3E78A48129867AC3963017531";//"D17AB6F0E89A42A8BFA26C57EE6396B2";
//        public static string MerchantSecretWord = "0c1ee50166";//"bd1e96fd16";
//        public static string MerchantSkinFolder = "SKINTEST";
//        public static string RefundCompanyPspID = "";
//        //public static string MerchantEndPointUrl = "https://www.apsp.biz:9085/merchanttools.asmx";
//        public static string BaseAPIURL = "http://localhost:61829/";
//        //public static string WorkAttendWebAppURL = "http://localhost:61829/";
//        public static string LoggingFilePath = "C:\\logs";
//        public static bool DoLogging = true;
//    }
//public class SiteTokenModel
//    {
//        public object data { get; set; }
//        public string msgCode { get; set; }
//        public string msgDescription { get; set; }
//        public string msgType { get; set; }
//    }
//public class PaymentTopupModel
//    {
//        public int SubscriptionId { get; set; }
//        public decimal Amount { get; set; }
//        public string baseURL { get; set; }
//    }
//public class PaymentResponseModel
//    {
//        public string Message { get; set; }
//        public bool IsOk { get; set; }
//        public decimal Amount { get; set; }
//    }
//    /// TODO: get these values from goabal config table
//    ///
//    public class APCOPaymentBase
//    {
//        public string MerchID { get { return GlobalAppConfigs.MerchantCode; } }
//        public string MerchPass { get { return GlobalAppConfigs.MerchantPassword; } }
//    }
//    public class APCOTransaction
//    {
//        public string RedirectionURL { get; set; }
//        public string status_url { get; set; }
//        public string FailedRedirectionURL { get; set; }
//        public string ProfileID { get { return GlobalAppConfigs.MerchantProfileId; } }
//        public int ActionType { get; set; } // 12 for refund
//       // public string ForceBank { get { return "PTEST"; } }  //TODO: comment this for live environment
//       // public string TEST { get; set; } //TODO: comment this for live environment
//        public decimal Value { get; set; }
//        public int Curr { get { return GlobalAppConfigs.MerchantCurrency; } }
//        public string Lang { get { return "en"; } }
//        public string ORef { get; set; }
//        public string PspID { get; set; }
//        public int? ClientAcc { get; set; }
//        public string Email { get; set; }
//        public string Address { get; set; }
//        public string RegCountry { get; set; }
//        public string UDF1 { get; set; }
//        public string UDF2 { get; set; }
//        public string UDF3 { get; set; }
      
//        //public string TopBannerURL { get { return "https://www.mscbookings.com/msc/images/marsalogo.jpg"; } }
//        //public string BottomBannerURL { get { return "https://www.mscbookings.com/msc/images/marsalogo.jpg"; } }
//        public string CSSTemplate { get; set; }
//        public string showDesc { get; set; }
//        public string return_pspid { get; set; }
//    }
//    public class APCOBuildTokenRequest : APCOPaymentBase
//    {
//        string encodedXMLRequest { get; set; }
//        public APCOBuildTokenRequest(string encodedXML)
//        {
//            encodedXMLRequest = encodedXML;
//        }
//        public string XMLParam { get { return encodedXMLRequest; } }
//    }
//    [Serializable, XmlRoot(ElementName = "BuildXMLTokenResponse")]
//    [XmlType("BuildXMLTokenResponse")]
//    public class APCOBuildTokenResponse
//    {
//        [XmlElement(ElementName = "BuildXMLTokenResult")]
//        public string BuildXMLTokenResult { get; set; }
//    }
//    public class APCOTransactionResponse
//    {
//        public string Result { get; set; }
//        public string ErrorMsg { get; set; }
//        public string BaseURL { get; set; }
//        public string Token { get; set; }
//    }
//    public class APCOTransactionStatusRequest : APCOPaymentBase
//    {
//        public string ORef { get; set; }
//    }
//    public class APCOTransactionStatusByORefRequest
//    {
//        public string MCHCode
//        {
//            get
//            {
//                return GlobalAppConfigs.MerchantCode;
//            }
//        }
//        public string MCHPass
//        {
//            get
//            {
//                return GlobalAppConfigs.MerchantPassword;
//            }
//        }
//        public string Oref { get; set; }
//    }
//    public class APCODailyTransactionsRequest
//    {
//        public string MCHCode
//        {
//            get
//            {
//                return GlobalAppConfigs.MerchantCode;
//            }
//        }
//        public string MCHPass
//        {
//            get
//            {
//                return GlobalAppConfigs.MerchantPassword;
//            }
//        }
//        public string sDate { get; set; }
//    }
//    public class APCOTransactionStatusResponse
//    {
//        public string Response { get; set; }
//    }
//    [Serializable, XmlRoot(ElementName = "TransactionResult")]
//    [XmlType("TransactionResult")]
//    public class APCOTransactionStatusModel
//    {
//        [XmlElement(ElementName = "ORef")]
//        public string ORef { get; set; }
//        [XmlElement(ElementName = "Result")]
//        public string Result { get; set; }
//        [XmlElement(ElementName = "AuthCode")]
//        public string AuthCode { get; set; }
//        [XmlElement(ElementName = "CardInput")]
//        public string CardInput { get; set; }
//        [XmlElement(ElementName = "pspid")]
//        public string pspid { get; set; }
//        [XmlElement(ElementName = "accepted")]
//        public string accepted { get; set; }
//        [XmlElement(ElementName = "Currency")]
//        public string Currency { get; set; }
//        [XmlElement(ElementName = "Value")]
//        public decimal Value { get; set; }
//        [XmlElement(ElementName = "Email")]
//        public string Email { get; set; }
//        [XmlElement(ElementName = "ExtendedData")]
//        public APCOTransactionStatusModelExtendedData ExtendedData { get; set; }
//    }
//    public class APCOTransactionStatusModelExtendedData
//    {
//        [XmlElement(ElementName = "CardNum")]
//        public string CardNum { get; set; }
//        [XmlElement(ElementName = "CardHName")]
//        public string CardHName { get; set; }
//        [XmlElement(ElementName = "Acq")]
//        public string Acq { get; set; }
//        [XmlElement(ElementName = "Source")]
//        public string Source { get; set; }
//    }
//    [Serializable, XmlRoot(ElementName = "Table1")]
//    [XmlType("Table1")]
//    public class APCO3DSecureTransactionResponse
//    {
//        [XmlElement(ElementName = "PSPID")]
//        public string PSPID { get; set; }
//        [XmlElement(ElementName = "TrnDate")]
//        public string TrnDate { get; set; }
//        [XmlElement(ElementName = "TrnType")]
//        public string TrnType { get; set; }
//        [XmlElement(ElementName = "BankCode")]
//        public string BankCode { get; set; }
//        [XmlElement(ElementName = "UserIP")]
//        public string UserIP { get; set; }
//        [XmlElement(ElementName = "CardNum")]
//        public string CardNum { get; set; }
//        [XmlElement(ElementName = "ExpDate")]
//        public string ExpDate { get; set; }
//        [XmlElement(ElementName = "CardHname")]
//        public string CardHname { get; set; }
//        [XmlElement(ElementName = "CardType")]
//        public string CardType { get; set; }
//        [XmlElement(ElementName = "CurrencyCode")]
//        public string CurrencyCode { get; set; }
//        [XmlElement(ElementName = "Amount")]
//        public decimal Amount { get; set; }
//        [XmlElement(ElementName = "BankAccept")]
//        public string BankAccept { get; set; }
//        [XmlElement(ElementName = "BankResponse")]
//        public string BankResponse { get; set; }
//        [XmlElement(ElementName = "AuthCode")]
//        public string AuthCode { get; set; }
//        [XmlElement(ElementName = "CardIssuerBank")]
//        public string CardIssuerBank { get; set; }
//        [XmlElement(ElementName = "CountryIP")]
//        public string CountryIP { get; set; }
//        [XmlElement(ElementName = "CountryBIN")]
//        public string CountryBIN { get; set; }
//        [XmlElement(ElementName = "CountryREG")]
//        public string CountryREG { get; set; }
//        [XmlElement(ElementName = "UDF1")]
//        public string UDF1 { get; set; }
//        [XmlElement(ElementName = "UDF2")]
//        public string UDF2 { get; set; }
//        [XmlElement(ElementName = "UDF3")]
//        public string UDF3 { get; set; }
//        [XmlElement(ElementName = "OrderRef")]
//        public string OrderRef { get; set; }
//        [XmlElement(ElementName = "Email")]
//        public string Email { get; set; }
//        [XmlAttribute(AttributeName = "id", Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1")]
//        public string Id { get; set; }
//        [XmlAttribute(AttributeName = "rowOrder", Namespace = "urn:schemas-microsoft-com:xml-msdata")]
//        public string RowOrder { get; set; }
//        [XmlAttribute(AttributeName = "hasChanges", Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1")]
//        public string HasChanges { get; set; }
//        [XmlAttribute(AttributeName = "msdata", Namespace = "http://www.w3.org/2000/xmlns/")]
//        public string Msdata { get; set; }
//        [XmlAttribute(AttributeName = "diffgr", Namespace = "http://www.w3.org/2000/xmlns/")]
//        public string Diffgr { get; set; }
//    }
//    //APCO DAILY TRANSACTIONS
//    [XmlRoot(ElementName = "getDailyTransactionSetNewResponse")]
//    public class GetDailyTransactionSetNewResponse
//    {
//        [XmlElement(ElementName = "getDailyTransactionSetNewResult")]
//        public string GetDailyTransactionSetNewResult { get; set; }
//        [XmlAttribute(AttributeName = "xmlns")]
//        public string Xmlns { get; set; }
//        [XmlText]
//        public string Text { get; set; }
//    }
//    [XmlRoot(ElementName = "Body")]
//    public class GetDailyTransactionSetNewBody
//    {
//        [XmlElement(ElementName = "getDailyTransactionSetNewResponse")]
//        public GetDailyTransactionSetNewResponse GetDailyTransactionSetNewResponse { get; set; }
//    }
//    [XmlRoot(ElementName = "Envelope")]
//    public class GetDailyTransactionSetNewEnvelope
//    {
//        [XmlElement(ElementName = "Body")]
//        public GetDailyTransactionSetNewBody Body { get; set; }
//        [XmlAttribute(AttributeName = "soap")]
//        public string Soap { get; set; }
//        [XmlAttribute(AttributeName = "xsi")]
//        public string Xsi { get; set; }
//        [XmlAttribute(AttributeName = "xsd")]
//        public string Xsd { get; set; }
//        [XmlText]
//        public string Text { get; set; }
//    }
//}

