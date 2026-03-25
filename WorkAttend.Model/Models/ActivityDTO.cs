using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class ActivityDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class ProjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    //public class SwitchActivity
    //{
    //    public int FromActivityId { get; set; }
    //    public int ToActivityId { get; set; }
    //    public DateTime SwitchTime { get; set; }
    //}
    public class DashboardData
    {
        // Result Set 1: Single Row
        public DailyDashboardStat Today { get; set; }

        // Result Set 2: List of Rows
        public List<DailyDashboardStat> ThisWeek { get; set; }

        // Result Set 3: List of Rows
        public List<WeeklyDashboardStat> ThisMonth { get; set; }
    }

    public class DailyDashboardStat
    {
        // Maps to SQL Column: Date (DATE)
        public DateTime Date { get; set; }

        // Maps to SQL Column: DayName (VARCHAR)
        public string DayName { get; set; }

        // Maps to SQL Column: TotalHours (DECIMAL)
        // We use 'double' or 'decimal' for charts.
        public double TotalHours { get; set; }

        // Maps to SQL Column: TotalDuration (VARCHAR)
        // Example: "05:30:00"
        public string TotalDuration { get; set; }
    }

    public class WeeklyDashboardStat
    {
        // Maps to SQL Column: WeekLabel (VARCHAR)
        // Example: "Week 1"
        public string WeekLabel { get; set; }

        // Maps to SQL Column: DateRange (VARCHAR)
        // Example: "Nov 01 - Nov 07"
        public string DateRange { get; set; }

        // Maps to SQL Column: TotalHours (DECIMAL)
        public double TotalHours { get; set; }

        // Maps to SQL Column: TotalDuration (VARCHAR)
        // Example: "107:27:19"
        public string TotalDuration { get; set; }
    }
}

