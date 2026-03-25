using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class dakarPunch
    {
        public int punchType { get; set; }
        public DateTime punchTime { get; set; }
        public string locationName { get; set; }
        public string punchDate { get; set; }
        public string employeePunchTime { get; set; }
        public string picture { get; set; }
        public List<ActivityDTO> activities { get; set; }
        public List<ProjectDTO> projects { get; set; }
        //public string latitude { get; set; }
        // public string longitude { get; set; }
    }
    public class ActivitySummaryDTO
    {
        public string ReportDate { get; set; }
        public string ProjectName { get; set; }
        public string TotalTime { get; set; }
        public string ActivityName { get; set; }
        public string ActivityDuration { get; set; }
        public DateTime ActivityStartDate { get; set; }
        public DateTime ActivityEndDate { get; set; }
        public TimeSpan DailyStartTime { get; set; }
        public TimeSpan DailyEndTime { get; set; }
        public string LocationName { get; set; }
        public string Notes { get; set; }
        public string Picture { get; set; }
        public string Color { get; set; }

    }
}