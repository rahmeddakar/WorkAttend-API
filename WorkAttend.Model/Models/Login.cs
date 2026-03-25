using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class Login
    {
       // public string token { get; set; }
        public string deviceID { get; set; }
        public string manufacturer { get; set; }
    }

    public class LoginUser
    {
        public int companyID { get; set; }
        public string email { get; set; }
        public int employeeID { get; set; }
        public string manufacturerID { get; set; }
        public string deviceID { get; set; }
        public bool IsFirstLogin { get; set; }
    }

    public class companyURL
    {
        public string url { get; set; }
        public bool isExist { get; set; }
    }
}