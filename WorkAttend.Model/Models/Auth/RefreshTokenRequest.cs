namespace WorkAttend.Model.Models.Auth
{
    public class RefreshTokenRequest
    {
        public string companyURL { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}