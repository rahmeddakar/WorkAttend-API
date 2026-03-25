namespace WorkAttend.Model.Models
{
    public class employeeprofile
    {
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public DateTime? DOB { get; set; }
        public int? employeeID { get; set; }
        public int employeeProfileID { get; set; }
        public string employeeCode { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
