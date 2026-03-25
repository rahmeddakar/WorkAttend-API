namespace WorkAttend.Model.Models
{
    public class EmployeeLocationsPageData
    {
        public List<Employees> employeesData { get; set; } = new List<Employees>();
        public List<Location> locationsData { get; set; } = new List<Location>();
        public List<Employees> assignEmployeesData { get; set; } = new List<Employees>();
        public List<CheckinResetRule> checkinResetRules { get; set; } = new List<CheckinResetRule>();
        public int selectedCheckinResetRuleId { get; set; }
    }
}