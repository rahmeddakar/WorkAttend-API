namespace WorkAttend.Model.Models
{
    public class employeepunchhistory
    {
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public int employeeID { get; set; }
        public int employeePunchHistoryID { get; set; }
        public int locationID { get; set; }
        public DateTime punchTime { get; set; }
        public DateTime punchTimeCountry { get; set; }
        public DateTime punchTimeUTC { get; set; }
        public int punchType { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
        public string picture { get; set; }
        public string notes { get; set; }
        public bool ismanualpunch { get; set; }
        public int manualpunchId { get; set; }
    }
}