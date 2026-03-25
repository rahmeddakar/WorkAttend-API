namespace WorkAttend.Model.Models
{
    public class appexception
    {
        public int appExceptionID { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string innerexceptionmessage { get; set; }
        public string message { get; set; }
        public string originatedAt { get; set; }
        public string source { get; set; }
        public string stacktrace { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}
