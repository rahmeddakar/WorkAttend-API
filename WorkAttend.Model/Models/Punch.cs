using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace WorkAttend.Model.Models
{
    public class Punch
    {
        [JsonProperty("employeeCode")]
        public int employeeID { get; set; }
        public string locationName { get; set; }
        public int locationID { get; set; }
        public int departmentID { get; set; } = 0;
        public List<Employees> employees { get; set; } = new();
        public List<Location> locations { get; set; } = new();
        public List<PunchDetail> punchDetails { get; set; } = new();
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string dateFilter { get; set; }
        public int pageNo { get; set; } = 1;
        public int TotalRecords { get; set; } = 0;
        public bool includeDeletedData { get; set; } = false;
        public List<subscriptionfeature> companysubscriptionpackagefeatures { get; set; } = new();
    }

    public class createPunch
    {
        public int employeeID { get; set; }
        public string locationName { get; set; }
        public int locationID { get; set; }
        public int departmentID { get; set; } = 0;
        public List<Location> locations { get; set; } = new();
        public List<Employees> employees { get; set; } = new();
        public DateTime punchDate { get; set; }
        public int punchType { get; set; }
    }

    public class ManualPunch
    {
        public List<ManualPunchesModel> manualpuncheslist { get; set; } = new();
        public int departmentID { get; set; } = 0;
        public string dateFilter { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int pageNo { get; set; } = 1;
        public int TotalRecords { get; set; } = 0;
    }

    public class punch
    {
        public int employeeID { get; set; }
        public string dakarEmployeeCode { get; set; }
        public int punchID { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationCode { get; set; }
        public int punchHistoryID { get; set; }
        public int companyID { get; set; }
        public string peNumber { get; set; }
        public DateTime punchTime { get; set; }
        public int punchType { get; set; }
        public DateTime punchTimeUTC { get; set; }
        public DateTime punchTimeCountry { get; set; }
        public bool isSyncDakar { get; set; }
        public bool isDeleted { get; set; }
        public DateTime createdOn { get; set; }
        public string createdBy { get; set; }
        public DateTime updatedOn { get; set; }
        public string updatedBy { get; set; }
    }

    public class ManualPunchApprovalRequest
    {
        public int Id { get; set; }
        public bool Status { get; set; }
    }

    public class PunchHistoryExportRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsIncludeDelRecords { get; set; }
        public bool IsIncludeCoords { get; set; }
        public int DepartmentID { get; set; } = 0;
    }

    public class PunchHistoryExportResult
    {
        public bool IsAllowed { get; set; }
        public bool HasData { get; set; }
        public string Message { get; set; }
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "text/csv";
        public string FileName { get; set; } = "export.csv";
    }
    public class PunchHistoryExportFile
    {
        public bool HasData { get; set; }
        public string Message { get; set; }
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; } = "text/csv";
    }

    public class projectTimeSheet
    {
        public int employeeid { get; set; }
        public string employeeCode { get; set; }
        public string employeeName { get; set; }
        public int projectID { get; set; }
        public int projectName { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public DateTime startDateFilter { get; set; }
        public DateTime endDateFilter { get; set; }
    }

    public class timeSheet
    {
        public int employeeid { get; set; }
        public string employeeCode { get; set; }
        public string employeeName { get; set; }
        public DateTime punchTimeCountry { get; set; }
        public DateTime punchDate { get; set; }
        public int punchType { get; set; }
        public DateTime startDateFilter { get; set; }
        public DateTime endDateFilter { get; set; }
        public int locationID { get; set; }
    }

    public class timeSheetEmp
    {
        public int employeeid { get; set; }
        public string employeeCode { get; set; }
        public string employeeName { get; set; }
        public DateTime punchTimeCountry { get; set; }
        public DateTime punchDate { get; set; }
        public int punchType { get; set; }
        public DateTime startDateFilter { get; set; }
        public DateTime endDateFilter { get; set; }
        public int locationID { get; set; }
        public string locationName { get; set; }
        public string punchTime { get; set; }
        public string punchTypeVal { get; set; }
    }
    public class timeSheetPunchList
    {
        public List<timeSheetPunch> punchTimeSheetList { get; set; }
        public int employeeID { get; set; }
        public int locationID { get; set; }
        public List<Employees> employees { get; set; }
        public List<Location> locations { get; set; }
    }

    public class timeSheetPunch
    {
        public int employeeID { get; set; }
        public string employeeName { get; set; }
        public List<employeePunches> employeePunchesList { get; set; }
    }

    public class employeePunches
    {
        public string punchDate { get; set; }
        public string punchIn { get; set; }
        public string punchOut { get; set; }
    }

    public class timeSheetEmployee
    {
        public int employeeid { get; set; }
        public DateTime startDateFilter { get; set; }
        public DateTime endDateFilter { get; set; }
        public int locationID { get; set; }
    }

    public class projectTimeSheetList
    {
        public List<projectTimeSheet> projectTsList { get; set; }
        public int employeeID { get; set; }
        public int locationID { get; set; }
        public List<Employees> employees { get; set; }
        public List<Location> locations { get; set; }
    }
}