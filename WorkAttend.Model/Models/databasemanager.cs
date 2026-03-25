namespace WorkAttend.Model.Models
{
    public class databasemanager
    {
        public int databasemanagerID { get; set; }
        public bool isAssigned { get; set; }
        public int assignedToCompanyID { get; set; }
        public string databaseName { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
