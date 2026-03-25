namespace WorkAttend.Model.Models
{
    public class UserAccessContext
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string CompanyURL { get; set; } = string.Empty;

        public int CompanyId { get; set; }
        public int BaseCompanyId { get; set; }
        public int RoleId { get; set; }
        public string Policy { get; set; } = string.Empty;
    }
}
