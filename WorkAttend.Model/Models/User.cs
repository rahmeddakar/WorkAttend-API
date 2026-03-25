using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class User
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string deviceID { get; set; }
        public string manufacturer { get; set; }
    }

    public class userAppVersion
    {
        public int employeeID { get; set; }
        public string appVersion { get; set; }
        public string storeProvider { get; set; }
    }

    public class validateDevice
    {
        public int employeeID { get; set; }
        public string appVersion { get; set; }
        public string storeProvider { get; set; }
        public string deviceID { get; set; }
    }
    public class validateDeviceResp
    {
        public bool isInCorrectDevice { get; set; }
        public string message { get; set; }
      
    }


    public class appDetailModel
    {
        public string databaseName { get; set; }
        public string userCompanyID { get; set; }
    }
}