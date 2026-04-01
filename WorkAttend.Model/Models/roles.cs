namespace WorkAttend.Model.Models
{    public class Roles
    {
        public int roleID { get; set; }
        public string policy { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool isSystem { get; set; }
        public int companyID { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
