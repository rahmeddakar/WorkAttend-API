namespace WorkAttend.Model.Models
{
    public class Companyconfigurations
    {
        public int companyConfigID { get; set; }
        public int companyID { get; set; }
        public int industryId { get; set; }
        public string description { get; set; }
        public string contactNumber { get; set; }
        public string mobileApplicationKey { get; set; }
        public string companyURL { get; set; }
        public string databaseName { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}
