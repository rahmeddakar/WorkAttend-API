using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace WorkAttend.Model.Models
{
    public class passwordChange
    {
        public  int employeeID { get; set; }
        public  string password { get; set; }
        public SelectList EmployeeList { get; set; }
    }
}