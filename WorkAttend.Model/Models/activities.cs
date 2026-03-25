namespace WorkAttend.Model.Models
{
    public class activities
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string color { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
        public bool isActive { get; set; }
    }
}
