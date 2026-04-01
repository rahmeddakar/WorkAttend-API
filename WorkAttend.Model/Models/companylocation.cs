namespace WorkAttend.Model.Models
{
    public class companylocation
    {
        public int companyID { get; set; }
        public int companyLocationID { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public int locationID { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}