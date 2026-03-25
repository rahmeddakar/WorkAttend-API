namespace WorkAttend.Model.Models
{
    public class companysubscriptionpackagefeatures
    {
        public int Id { get; set; }
        public int companyId { get; set; }
        public int subscriptionpackageId { get; set; }
        public int subscriptionpackagefeatureId { get; set; }
        public int FeatureValue { get; set; }
        public bool isActive { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
        public bool IsDelete { get; set; }
    }
}
