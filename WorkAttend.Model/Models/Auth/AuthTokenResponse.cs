namespace WorkAttend.Model.Models.Auth
{
    public class AuthTokenResponse
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public int expiresInMinutes { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string companyURL { get; set; }
    }
}