using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace WorkAttend.Model.Models
{
    public class registerNewCompany
    {
        public string peNumber { get; set; }
        public string companyName { get; set; }
    }

    public class departmentComp
    {
        public int departmentID { get; set; }
        public string departmentCode { get; set; }
        public string departmentName { get; set; }
    }
}