using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace WorkAttend.Model.Models
{
    public class monthlyStat
    {
        public int total { get; set; }
        public string Month  { get; set; }
        public int Monthnumber  { get; set; }
    }
    public class emergencyList
    {
        public List<emergencyData> emergencyData { get; set; }
        public int assemblyPointID { get; set; }
        public string assemblyPoint { get; set; }
        public DateTime printedOn { get; set; }
    }
    public class emergencyData
    {
        public int employeeID { get; set; }
        public int employeepunchhistoryID { get; set; }
        public string firstname { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string locationName { get; set; }
        public DateTime punchDateTime { get; set; }
        public string mobileNumber { get; set; }
        public int  assemblypointID { get; set; }
        public string assemblypoint { get; set; }
    }
    public class Stats
    {
        public int employeeCount { get; set; }
        public int locationCount { get; set; }
        public int punchReqCount { get; set; }
        public int userDeviceCount { get; set; }
        public int employeeGlobalCount { get; set; }
        public int locationGlobalCount { get; set; }
        public int punchReqGlobalCount { get; set; }
        public int userDeviceGlobalCount { get; set; }
        public int emplLocGlobalCount { get; set; }
        public string timeRemaining { get; set; }
        public int inCount { get; set; }
        public int outCount { get; set; }
        public int onSiteCount { get; set; }
        public int activeLocations { get; set; }
        public int onSitePercentage { get; set; }
        public int activeLocationPercentage { get; set; }
        public int activeEmployees { get; set; }
        public int activeEmployeePercent { get; set; }
        public List<punchHistoryCSV> history {get; set;}
        public string dateFilter { get; set; }
        public string clockInMonthData { get; set; }
        public string clockOutMonthData { get; set; }
        public string locationMonthData { get; set; }
        public string activeDeviceMonthData { get; set; }
        public int punchInCount { get; set; }
        public int punchOutCount { get; set; }
        public List<Employees> employees { get; set; }
        public List<Location> companyLocation { get; set; }
        public List<UserDevice> employeeMobileDevices { get; set; }
        public List<punchHistoryCSV> latestPunchIn { get; set; }
        public List<ActivitySummaryDTO> activitySummary { get; set; }
        public List<Activity> Activities { get; set; }
        public List<ActivitySummaryDTO> projectSummary { get; set; }
        public List<Project> Projects { get; set; }
    }
    //public class ActivitySummaryDTO
    //{
    //    public string ActivityName { get; set; }
    //    public string ProjectName { get; set; }
    //    public string Color { get; set; }
    //    public string LocationName { get; set; }
    //    public DateTime? ClockInTime { get; set; }
    //    public DateTime? ActivityEndTime { get; set; }
    //    public int TotalMinutes { get; set; }
    //    public int Total { get; set; }
    //}
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int locationID { get; set; }
        public string LocationName { get; set; }

        public string Description { get; set; }
    }
    public class Activity {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
    }
}