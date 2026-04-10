using System;

namespace WorkAttend.Model.Models
{
    public class TimeSheetExportRequest
    {
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public bool isIncludeDelRecords { get; set; }
        public bool isIncludeCoords { get; set; }
        public int employeeID { get; set; }
    }
}