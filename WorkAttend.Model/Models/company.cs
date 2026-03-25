namespace WorkAttend.Model.Models
{
    public class company
    {
        public int companyId { get; set; }
        public int countryId { get; set; }
        public int baseCompanyId { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public bool IsDakarConnected { get; set; }
        public bool isDeleted { get; set; }
        public string name { get; set; }
        public string peNumber { get; set; }
        public int timezoneID { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}
