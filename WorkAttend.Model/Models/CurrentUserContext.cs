namespace WorkAttend.Model.Models
{
    public sealed class CurrentUserContext
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string CompanyURL { get; set; } = string.Empty;
    }
}
