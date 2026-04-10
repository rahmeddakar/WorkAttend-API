namespace WorkAttend.Model.Models
{
    public class TimeSheetCsvExportResult
    {
        public byte[] fileBytes { get; set; }
        public string contentType { get; set; }
        public string fileName { get; set; }
    }
}