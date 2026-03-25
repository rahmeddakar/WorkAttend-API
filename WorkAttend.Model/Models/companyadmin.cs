namespace WorkAttend.Model.Models
{
    public class companyadmin
    {        public int adminId { get; set; }
        public int companyAdminId { get; set; }
        public int companyId { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public bool IsSuperAdmin { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}
