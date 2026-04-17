namespace WorkAttend.Model.Models.Auth
{
    public class LoginResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string Policy { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public int BaseCompanyId { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresInMinutes { get; set; }
    }
}
