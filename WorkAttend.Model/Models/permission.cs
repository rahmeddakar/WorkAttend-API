namespace WorkAttend.Model.Models
{
    public class permission
    {
        public string controllerName { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public bool isDeleted { get; set; }
        public bool isMendatory { get; set; }
        public string Name { get; set; }
        public int permissionID { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
