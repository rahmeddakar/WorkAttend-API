namespace WorkAttend.Model.Models
{
    public class punchlocation
    {
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int punchHistoryID { get; set; }
        public int punchLocationID { get; set; }
        public int punchAttributeValueID { get; set; }
        public string punchAttributeValue { get; set; }
        public string updatedBy { get; set; }
        public DateTime updatedOn { get; set; }
    }
}