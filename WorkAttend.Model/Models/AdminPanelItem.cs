namespace WorkAttend.Model.Models
{
    public class AdminPanelItem
    {
        public long CompanyId { get; set; }
        public string Name { get; set; }
        public bool IsDakarConnected { get; set; }
        public string DatabaseName { get; set; }
        public string DisplayName { get; set; }
        public DateTime? PackageStartDate { get; set; }
        public DateTime? PackageEndDate { get; set; }
        public string CompanyURL { get; set; }
    }
}