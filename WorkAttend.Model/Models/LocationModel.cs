namespace WorkAttend.Model.Models
{
    public class LocationModel
    {
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public bool IsDeleted { get; set; }
        public double latitudeP1 { get; set; }
        public double latitudeP2 { get; set; }
        public double latitudeP3 { get; set; }
        public double latitudeP4 { get; set; }
        public int locationID { get; set; }
        public string locationName { get; set; }
        public string locationCode { get; set; }
        public double longitudeP1 { get; set; }
        public double longitudeP2 { get; set; }
        public double longitudeP3 { get; set; }
        public double longitudeP4 { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }
}