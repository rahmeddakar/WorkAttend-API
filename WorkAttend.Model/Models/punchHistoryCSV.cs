using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace WorkAttend.Model.Models
{
    public class punchHistoryCSV
    {
        public int employeepunchHistoryID { get; set; }
        public string employeeCode { get; set; }
        public string firstname { get; set; }
        public string surname { get; set; }
        public string departmentName { get; set; }
        public string punchdate { get; set; }
        public string punchtime { get; set; }
        public string punchType { get; set; }
       
        public int locationID { get; set; }

        public string locationCode { get; set; }

        public string locationName { get; set; }
        public string locormanual { get; set; }
        public string departmentCode { get; set; }
        public string employeeUniqueIdentity { get; set; }
    }
    public class punchHistoryCoordsCSV
    {
        public string employeeCode { get; set; }
        public string firstname { get; set; }
        public string surname { get; set; }
        public string departmentName { get; set; }
        public string punchdate { get; set; }
        public string punchtime { get; set; }
        public string punchType { get; set; }
        
        public int locationID { get; set; }

        public string locationCode { get; set; }

        public string locationName { get; set; }

        public string locormanual { get; set; }
        public string departmentCode { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string employeeUniqueIdentity { get; set; }
    }
}