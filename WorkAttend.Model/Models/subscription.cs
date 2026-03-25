using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace WorkAttend.Model.Models
{
    public class subscription
    {
        public string currentPackageName { get; set; }
        public DateTime currentPackageValid { get; set; }
        public List<subscriptionPackages> packages { get; set; }
        public companysubscriptionpackage currentPackageData { get; set; }
        public companyattribute address { get; set; }
        public bool isConfigurable { get; set; }
        public List<SubscriptionFeatureRequestModel> subscriptionpackagefeatures { get; set; }
        public Dictionary<int,bool> configurableFeatureId { get; set; }
    }

    public class subscriptionPackages
    {
        public int packageID { get; set; }
        public string packageName { get; set; }
        public string description { get; set; }
        public List<string> features { get; set; }
        public decimal pricePerMonth { get; set; }
        public decimal pricePerYear { get; set; }
    }

    public class ConfigurableFeature
    {
        public int featureId { get; set; }
        public bool status { get; set; }
    }

    public class subscribePlan
    {
        public int numberOfEmployees { get; set; }
        public int packageID { get; set; }
        public string monthlyorAnnual { get; set; }
    }

    public class billingAddress
    {
        //public string name { get; set; }
        //public string email { get; set; }
        public int companyID { get; set; }
        public int currencyID { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string city { get; set; }
        public string postcode { get; set; }
        public string phoneNumber { get; set; }
        public bool status { get; set; }
        public int featureId { get; set; }
    }
}