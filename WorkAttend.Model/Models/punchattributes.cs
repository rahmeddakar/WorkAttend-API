namespace WorkAttend.Model.Models
{
    public class punchattributes
    {
        public int punchAttributeID { get; set; }
        public int companyID { get; set; }
        public string name { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public bool isCollectDaily { get; set; }
        public bool isMobileAppEnabled { get; set; }
        public bool isDeleted { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}