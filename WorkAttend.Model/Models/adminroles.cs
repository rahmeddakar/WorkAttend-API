namespace WorkAttend.Model.Models
{    public class adminroles
    {
        public int adminRoleID { get; set; }
        public int adminID { get; set; }
        public int companyID { get; set; }
        public int roleID { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}
