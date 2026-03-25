namespace WorkAttend.Model.Models
{
    public class subscriptionpackage
    {
        public int subscriptionPackageID { get; set; }
        public string name { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public string numberOfDays { get; set; }
        public decimal pricePerMonth { get; set; }
        public decimal pricePerYear { get; set; }
        public string currencyID { get; set; }
        public bool isSystem { get; set; }
        public bool isActive { get; set; }
        public bool isDeleted { get; set; }
        public string createdOn { get; set; }
        public string createdBy { get; set; }
        public string updatedOn { get; set; }
        public string updatedBy { get; set; }
    }
}
