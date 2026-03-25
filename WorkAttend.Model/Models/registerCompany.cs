using Microsoft.AspNetCore.Mvc.Rendering;
namespace WorkAttend.Model.Models
{
    public class registerCompany
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string description { get; set; }
        public string phoneNumber { get; set; }
        public string peNumber { get; set; }
        public string companyName { get; set; }
        public string adminEmail { get; set; }
        public string adminName { get; set; }
        public string adminPassword { get; set; }
        public bool isDakarConnected { get; set; }
        public SelectList subscriptions { get; set; }
        public int subscriptionPackageID { get; set; }
        public SelectList countries { get; set; }
        public int countryID { get; set; }
        public SelectList currencies { get; set; }
        public int CurrencyTypeID { get; set; }
        public SelectList industries { get; set; }
        public int industryID { get; set; }
        public string companyURL { get; set; }
        public string accountType { get; set; }
        public string expectedNumOfEmp { get; set; }
    }
}