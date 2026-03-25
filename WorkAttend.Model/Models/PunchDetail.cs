using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace WorkAttend.Model.Models
{
    public class SinglePunch
    {
        public int punchHistoryInId { get; set; }
        public int punchHistoryOutId { get; set; }
        public string timeIn { get; set; }
        public string timeOut { get; set; }
        public string ActualHours { get; set; }
        public string longitudeIn { get; set; }
        public string latitudeIn { get; set; }
        public string longitudeOut { get; set; }
        public string latitudeOut { get; set; }
        public string pictureIn { get; set; }
        public string pictureOut { get; set; }
        public string notesIn { get; set; }
        public string notesOut { get; set; }
        public int punchtype { get; set; }
    }
        public class PunchDetail
    {
        public int punchId { get; set; }
        public int employeeID { get; set; }
        public string employeeName { get; set; }
        public string locationName { get; set; }
        public int locationId { get; set; }
        public string punchInOutDate { get; set; }
        public SinglePunch[] punchIn { get; set; }
        public SinglePunch[] punchOut { get; set; }
        public Dictionary<string, SinglePunch> punchInOut { get; set; }
        public string totalHours { get; set; }
        public int punchType { get; set; }
        public string dayName { get; set; }
        public string grandTotal { get; set; }
    }
}