namespace WorkAttend.Model.Models
{
    public class employeeattribute
    {
        public int attributeID { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public int employeeID { get; set; }
        public int employeeProfileAttributeID { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
        public string Value { get; set; }
    }
}
