using System;

namespace WorkAttend.Model.Models
{
    public class projectClient
    {
        public int projectClientID { get; set; }
        public int projectID { get; set; }
        public int clientID { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}