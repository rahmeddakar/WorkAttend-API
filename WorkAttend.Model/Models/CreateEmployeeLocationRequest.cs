namespace WorkAttend.Model.Models
{
    public class CreateEmployeeLocationRequest
    {
        public int employeeID { get; set; }
        public int locationID { get; set; }
        public int departmentID { get; set; }
    }
}