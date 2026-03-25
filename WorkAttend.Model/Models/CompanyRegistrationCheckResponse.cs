namespace WorkAttend.Model.Models
{
    public class CompanyRegistrationCheckResponse
    {
        public bool isExist { get; set; }
        public bool urlAlreadyExist { get; set; }
        public string message { get; set; }
    }
}