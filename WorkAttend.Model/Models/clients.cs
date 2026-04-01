namespace WorkAttend.Model.Models
{
    public class clients
    {
        public int clientID { get; set; }
        public int companyID { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}
