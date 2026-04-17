using System;

namespace WorkAttend.Model.Models.Auth
{
    public class adminrefreshtoken
    {
        public int adminrefreshtokenID { get; set; }
        public int adminID { get; set; }
        public string token { get; set; }
        public DateTime expiryDate { get; set; }
        public bool isDeleted { get; set; }
        public DateTime createdOn { get; set; }
        public string createdBy { get; set; }
        public DateTime updatedOn { get; set; }
        public string updatedBy { get; set; }
    }
}