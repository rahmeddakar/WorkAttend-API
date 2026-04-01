namespace WorkAttend.Model.Models.Auth
{
    public class ValidateResetTokenRequest
    {
        public string companyURL { get; set; }
        public string token { get; set; }
        public string identity { get; set; }
    }
}