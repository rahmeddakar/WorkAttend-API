using System.Collections.Generic;

namespace WorkAttend.Model.Models
{
    public class EmployeePageData
    {
        public List<Employees> employeeList { get; set; } = new List<Employees>();
        public List<department> departments { get; set; } = new List<department>();
        public List<Jobs> employeeJobs { get; set; } = new List<Jobs>();
    }
}