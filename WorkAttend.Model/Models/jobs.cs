namespace WorkAttend.Model.Models
{
    public class jobs
    {
        public int Id { get; set; }
        public string Job { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
        public bool isActive { get; set; }
    }
}