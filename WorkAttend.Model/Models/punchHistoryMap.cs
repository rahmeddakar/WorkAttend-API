using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace WorkAttend.Model.Models
{
    public class punchHistoryMap
    {
        public int employeeID { get; set; }
        public SelectList EmployeeList { get; set; }
        public DateTime date { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public List<Location> locations {get; set;}
        public List<punchLocationMarkers> punchlocations { get; set; }
        public string dateFilter { get; set; }
    }
    public class punchLocationMarkers
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        public int employeepunchhistoryid { get; set; }
        public string locationName { get; set; }
        public DateTime punchTime { get; set; }
        public int punchType { get; set; }
     }

    public class punchTimesheetList
    {
         public string createdBy { get; set; }
         public DateTime? createdOn { get; set; }
         public int employeeID { get; set; }
         public int employeePunchHistoryID { get; set; }
         public int locationID { get; set; }
         public DateTime punchTime { get; set; }
         public DateTime punchTimeCountry { get; set; }
         public DateTime punchTimeUTC { get; set; }
         public int punchType { get; set; }
         public string updatedBy { get; set; }
         public DateTime? updatedOn { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string picture { get; set; }
        public string notes { get; set; }
    }
}