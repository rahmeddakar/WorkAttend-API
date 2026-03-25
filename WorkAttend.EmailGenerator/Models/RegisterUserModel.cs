using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.DataServices;

namespace WorkAttend.EmailGenerator.Models
{
    public class BaseModel
    {
        public EmailToken EmailToken { get; set; }
        public string PersonName { get; set; }
        public string EmailImagesPath = "";
        public string WebSiteURL = "";
        public bool IsSubscriptionRequest { get; set; }
        public string ErrorMsg { get; set; }
        public int UserID { get; set; }
        public short LangID { get; set; }
        public bool IsPointRequest { get; set; }
    }

  
    public class RegisterUserModel : BaseModel
    {
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string ActivationURL { get; set; }
        public string Password { get; set; }
        public string WebsiteURL { get; set; }
        public string Mobile { get; set; }
        public string PicURL { get; set; }
        public string UserName { get; set; }
        public string AccountNo { get; set; }
        public string InvoiceNo { get; set; }
        public string VerifyEmailLink { get; set; }
        public string PhoneNo { get; set; }
        public string StoreName { get; set; }

    }

    public class ContactUsModel
    {
        public string BusinessName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNo { get; set; }
        public string Country { get; set; }
        public string BusinessType { get; set; }
        public string Locations { get; set; }
    }

    public class SubPaymentModel
    {
        public string ContactUs { get; set; }
        public string LoginURL { get; set; }
        public string PlanPrice { get; set; }
        public string PlanName { get; set; }
        public string NextPaymentDate { get; set; }
        public string UserName { get; set; }
        public int NbrOfOutlet { get; set; }
        public int NbrOfRegister { get; set; }
        public string HasAnalytics { get; set; }
        public string StoreName { get; set; }
        public string Currency { get; set; }
    }

    public class ForgetPassModel
    {
        public string PassCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AccReminderPassModel
    {
        public string CustomerCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal? AccountCredit { get; set; }
        public decimal? TotalAccount { get; set; }
        public string StoreName { get; set; }
    }
}