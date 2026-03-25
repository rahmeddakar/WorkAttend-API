using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WorkAttend.Model.Models
{
    public class Schedule
    {
        public int? scheduleId { get; set; }
        public int employeeID { get; set; }
        public int? locationID { get; set; }
        public DateTime timeIn { get; set; }
        public DateTime timeOut { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public SelectList locationList { get; set; }
        public string timeInString { get; set; }
        public string timeOutString { get; set; }
    }
    public class LocationList
    {
        public string locationCode { get; set; }
        public string name { get; set; }
    }
    public class EmployeeList
    {
        public string employeeCode { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string locationName { get; set; }
    }
}
