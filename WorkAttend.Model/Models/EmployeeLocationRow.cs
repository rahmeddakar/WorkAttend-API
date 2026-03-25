namespace WorkAttend.Model.Models
{
    public class EmployeeLocationRow
    {
        public int employeeLocationID { get; set; }
        public int employeeID { get; set; }
        public string firstName { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string locationName { get; set; }
        public int locationID { get; set; }

        public string fullName
        {
            get
            {
                return $"{firstName} {surname}".Trim();
            }
        }
    }
}