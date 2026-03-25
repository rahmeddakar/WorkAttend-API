using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace WorkAttend.Model.Models
{
    public class UserDevice
    {
        [JsonProperty("employeeMobileAppID")]
        public int employeeMobileAppID { get; set; }
        public int employeeID { get; set; }
        public string employeeName { get; set; }
        public string manufacturerID { get; set; }
        public bool isDeleted { get; set; }
        public List<UserDevice> userDevicesList { get; set; }
        public string email { get; set; }
        public string appVersion { get; set; }
    }
}