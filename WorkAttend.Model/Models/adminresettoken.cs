namespace WorkAttend.Model.Models
{
    public class adminresettoken
    {
        public int adminresettokenID { get; set; }
        public int adminID { get; set; }
        public string token { get; set; }
        public DateTime expiryDate { get; set; }
        public bool isDeleted { get; set; }
        public DateTime createdOn { get; set; }
        public string createdBy { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}