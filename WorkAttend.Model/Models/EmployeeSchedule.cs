using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace WorkAttend.Model.Models
{
    public class EmployeeSchedule
    {
        [JsonProperty("employeeScheduleID")]
        public int employeeScheduleID { get; set; }
        [JsonProperty("employeeID")]
        public int employeeID { get; set; }
        [JsonProperty("locationId")]
        public int locationId { get; set; }
        [JsonProperty("locationName")]
        public string locationName { get; set; }
        [JsonProperty("timeIn")]
        public DateTime timeIn { get; set; }
        [JsonProperty("timeOut")]
        public DateTime timeOut { get; set; }
        [JsonProperty("firstName")]
        public string firstName { get; set; }
        [JsonProperty("lastName")]
        public string lastName { get; set; }
    }
}