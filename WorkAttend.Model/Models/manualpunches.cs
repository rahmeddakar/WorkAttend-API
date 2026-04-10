namespace WorkAttend.Model.Models
{
    public class manualpunches
    {
        public int id { get; set; }
        public int employeeId { get; set; }
        public int locationId { get; set; }
        public DateTime punchTime { get; set; }
        public int punchType { get; set; }
        public string reason { get; set; }
        public bool isApproved { get; set; }
        public DateTime? createdOn { get; set; }
        public string createdBy { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}
