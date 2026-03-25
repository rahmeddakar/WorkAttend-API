namespace WorkAttend.Model.Models
{
    public class subscriptionpackagefeatures
    {
        public int SubscriptionPackageFeatureID { get; set; }
        public int SubscriptionPackageID { get; set; }
        public int SubscriptionFeatureID { get; set; }
        public int FeatureValue { get; set; }
        public string FeatureValueDetail { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSystem { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
