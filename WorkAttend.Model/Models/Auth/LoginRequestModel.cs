namespace WorkAttend.Model.Models.Auth
{
    public class LoginRequestModel
    {
        public string CompanyURL { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
