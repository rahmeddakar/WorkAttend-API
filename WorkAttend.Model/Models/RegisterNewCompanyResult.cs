namespace WorkAttend.Model.Models
{
    public class RegisterNewCompanyResult
    {
        public int companyId { get; set; }
        public int departmentId { get; set; }
        public int adminId { get; set; }
        public string companyName { get; set; }
        public string message { get; set; }
    }
}