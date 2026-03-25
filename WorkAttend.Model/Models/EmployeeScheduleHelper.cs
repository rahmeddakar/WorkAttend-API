using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace WorkAttend.Model.Models
{
    public class EmployeeScheduleHelper
    {
        [JsonProperty("employeeID")]
        public int employeeID { get; set; }
        [JsonProperty("firstName")]
        public string firstName { get; set; }
        [JsonProperty("surname")]
        public string surname { get; set; }
        public string locationName { get; set; }
        public int locationID { get; set; }
        public SelectList EmployeeList { get; set; }
        public SelectList locationList { get; set; }
        public List<EmployeeSchedule> employeeSchedules { get; set; }
        public string fullname
        {
            get
            {
                return string.Format("{0} {1}", firstName, surname);
            }
        }
    }
}