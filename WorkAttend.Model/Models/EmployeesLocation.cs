namespace WorkAttend.Model.Models
{
    public class EmployeesLocation
    {
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public int employeeID { get; set; }
        public int employeeLocationID { get; set; }
        public bool isDeleted { get; set; }
        public int LocationID { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}

