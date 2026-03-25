using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{

    public class ManualPunchUser
    {
        public int EmployeeID { get; set; }
        public int LocationId { get; set; }
        //public double latitude { get; set; }
        //public double longitude { get; set; }
        public int PunchType { get; set; }
        public string punchTime { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset? DateFrom { get; set; } = null;
        public DateTimeOffset? DateTo { get; set; }=null;
    }
    public class GeoFenceUser
    {
        public int employeeID { get; set; }
        public int locationId { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int punchType { get; set; }
        public string employeeEmail { get; set; }
        public int attributeValueID { get; set; }
        public string attributeValue { get; set; }
        public string picture { get; set; }
        public string notes { get; set; }
        public DateTime PunchDateTime { get; set; }
        public int activityId { get; set; }
        public int projectId { get; set; }
        public string jobIds{ get; set; }
    }

    public class checkGeoFence
    {
        public int employeeID { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}