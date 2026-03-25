using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class GeoPunch
    {
        // public string dakarUserToken { get; set; }
        //public string dakarUserName { get; set; }
        public string employeeEmail { get; set; }
        public int employeeID { get; set; }
        public int locationID { get; set; }
        public DateTime punchDate { get; set; }
        public DateTime punchTime { get; set; }
        public int punchType { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }

    }

    //public class ActivitySummaryRequest
    //{
    //    public string StartDate { get; set; }
    //    public string EndDate { get; set; }
    //}
}