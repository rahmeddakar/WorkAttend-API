using System.Collections.Generic;

namespace WorkAttend.Shared.Helpers
{
    public static class Constants
    {
        //public const string createGeoFencePermissions = "createGeoFence";
        //public const string createEmployeePermissions = "createEmployee";
        //public const string createPunchPermissions = "createPunch";
        //public const string createSchedulePermissions = "createSchedule";
        //public const string deleteGeoFence = "deleteGeoFence";
        //public const string deleteHistory = "deleteHistory";
        //public const string showHistory = "showPunchHistory";
        public static Dictionary<int, string> permissionList = new Dictionary<int, string>();
        public const string ACTION_VIEW = "View";
        public const string ACTION_CREATE = "Create";
        public const string ACTION_UPDATE = "Update";
        public const string ACTION_DELETE = "Delete";
        public const string ACTION_EXPORT = "Export";
        public static Dictionary<string, string> headingList = new Dictionary<string, string>()
        {
        { "/home/index","Dashboard"},
        { "/punchhistory/index","TimeSheet"},
        { "/geofence/index","Locations"},
        { "/employee/employeelocations","Assign Locations"},
        { "/employee/index","Employees"},
        { "/admins/index","Managers"},
        { "/admins/role","Manager Roles and Rights"},
        { "/questionnaire/index","Questionnaire"},
        { "/punchactivity/index","Activities"},
          { "/punchhistory/manualpunch","Manual Punch Requests"},
        };
        public enum PermissionSubscription
        {
            NotValidPermission = 1,
            NotSubscription = 2,
            ValidPermission = 3
        }
        public enum WorkAttendMenu
        {
            Dashboard = 1,
            Timesheets = 2,
            Geofences = 3,
            Employees = 4,
            Managers = 5,
            Settings = 6
        };
        public enum Features
        {
            Employees = 1,
            Departments = 2,
            Company = 3,
            ExportData = 4,
            Geofences = 5,
            Admins = 6,
            FacePunch = 7,
            Notes = 8,
            ManualPunch = 9,
            Activity = 10,
            Job = 11
        }
    }
}