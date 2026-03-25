using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkAttend.Model.Models
{
    public class employeemobileapp
    {
        public string appVersion { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string deviceID { get; set; }
        public int employeeID { get; set; }
        public int employeeMobileAppId { get; set; }
        public bool isDeleted { get; set; }
        public string manufacturerID { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}
