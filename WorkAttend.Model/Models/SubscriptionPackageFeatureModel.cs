namespace WorkAttend.Model.Models
{
    public class SubscriptionPackageFeatureModel
    {
        public int SubscriptionFeatureID { get; set; }
        public bool IsActive { get; set; }
        public DateTime packageStartDate { get; set; }
        public DateTime packageEndDate { get; set; }
        public int FeatureValue { get; set; }
    }
    public class SubscriptionFeatureRequestModel
    {
        public string FeatureName { get; set; }
        public string DisplayName { get; set; }
        public string FeatureDescription { get; set; }
        public int SubscriptionFeatureID { get; set; }
        public string FeatureValue { get; set; }
        public string FeatureValueDetail { get; set; }
        public bool IsActive { get; set; }

    }
}