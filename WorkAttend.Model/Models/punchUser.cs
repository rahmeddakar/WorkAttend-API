using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class punchUser
    {
      //  public string dakarUserName { get; set; }
       // public string dakarEmployeeID { get; set; }
        //public string dakarUserToken { get; set; }
        public int companyID { get; set; }
        public int employeeID { get; set;  }
        public int locationID { get; set; }
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
        public string picture { get; set; }
    }
}