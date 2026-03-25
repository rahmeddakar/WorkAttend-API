namespace WorkAttend.Model.Models
{
    public class employeeJobs
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }

        public int JobId { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
        public bool isActive { get; set; }
    }
}
