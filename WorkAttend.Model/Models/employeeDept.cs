using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace WorkAttend.Model.Models
{
    public class employeeDept
    {
        public int employeeID { get; set; }
        public string firstname { get; set; }
        public string surname { get; set; }
        public string departmentName { get; set; }
        public int companyID { get; set; }
        public int departmentID { get; set; }
    }
    public class employeeDepartmentModel
    {
        public List<employeeDept> empDepts { get; set; }
        public SelectList departmentList { get; set; } = null;
        public int departmentIDUpdate { get; set; } = 0;
        public int selectedEmployeeID { get; set; }
        public int oldDeptId { get; set; }
    }
}