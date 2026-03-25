namespace WorkAttend.Model.Models
{
    public class Employees
    {
        public int employeeID { get; set; }
        public string employeeCode { get; set; }
        public string companyName { get; set; }
        public string createdBy { get; set; }
        public string email { get; set; }
        public int companyID { get; set; }
        public string firstName { get; set; }
        public string surName { get; set; }
        public DateTime dob { get; set; }
        public string empDisplayName { get; set; }
        public string departmentName { get; set; }
        public DateTime createdOn { get; set; }
        public List<employeemobileapp> mobileDevices { get; set; }
        public string manufacturerID { get; set; }
        public string appVersion { get; set; }
        public int departmentID { get; set; }
        public string password { get; set; }
        public string employeeJobs { get; set; }
        public bool isForAdd { get; set; }
    }

    public class EmployeeModel
    {
        public List<Employees> employeeList { get; set; } = new List<Employees>();
        public List<department> departments { get; set; } = new List<department>();
        public List<Jobs> employeeJobs { get; set; } = new List<Jobs>();
        public int departmentID { get; set; }
        public List<int> jobID { get; set; } = new List<int>();
    }

    public class createEmployeeModel
    {
        public List<department> departments { get; set; } = new List<department>();
    }

    public class addEmployeeModel
    {
        public int companyID { get; set; }
        public string emailEmployee { get; set; }
        public string passwordEmployee { get; set; }
        public string firstName { get; set; }
        public string surName { get; set; }
        public DateTime dob { get; set; }
        public int departmentID { get; set; }
        public string mobileNumber { get; set; }
        public string employeeCode { get; set; }
        public string employeeJobs { get; set; }
        public bool isForAdd { get; set; }
    }
}