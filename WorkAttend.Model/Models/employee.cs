namespace WorkAttend.Model.Models
{
    public class employee
    {
        public int companyID { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string email { get; set; }
        public int employeeID { get; set; }
        public bool isDeleted { get; set; }
        public bool IsMobile { get; set; }
        public string password { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}
