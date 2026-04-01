namespace WorkAttend.Model.Models.Auth
{
    public class UpdatePasswordRequest
    {
        public string companyURL { get; set; }
        public string token { get; set; }
        public string identity { get; set; }
        public string password { get; set; }
    }
}