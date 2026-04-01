using Newtonsoft.Json;

namespace WorkAttend.Model.Models
{
    public class Punch
    {
        [JsonProperty("employeeCode")]
        public int employeeID { get; set; }
        public string locationName { get; set; }
        public int locationID { get; set; }
        public List<Employees> employees { get; set; } = new List<Employees>();
        public List<Location> locations { get; set; } = new List<Location>();
        public List<PunchDetail> punchDetails { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string dateFilter { get; set; }
        public int pageNo { get; set; } = 1;
        public int TotalRecords { get; set; } = 0;
        public bool includeDeletedData { get; set; } = false;
        public List<subscriptionfeature> companysubscriptionpackagefeatures { get; set; } = null;
    }
    public class createPunch
    {
        public int employeeID { get; set; }
        public string locationName { get; set; }
        public int locationID { get; set; }
        //public SelectList EmployeeList { get; set; }
        //public SelectList locationList { get; set; }
        public List<Location> locations { get; set; }
        public List<Employees> employees { get; set; }
        public DateTime punchDate { get; set; }
        public int punchType { get; set; }
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

    public class timeSheetPunchList
    {
        public List<timeSheetPunch> punchTimeSheetList { get; set; }
        public int employeeID { get; set; }
        public int locationID { get; set; }
        public List<Employees> employees { get; set; } = new List<Employees>();
        public List<Location> locations { get; set; } = new List<Location>();

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
        public List<Employees> employees { get; set; } = new List<Employees>();
        public List<Location> locations { get; set; } = new List<Location>();
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
    public class ManualPunch
    {
        public List<ManualPunchesModel> manualpuncheslist { get; set; }
        public string dateFilter { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int pageNo { get; set; } = 1;
        public int TotalRecords { get; set; } = 0;
    }
}