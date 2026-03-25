namespace WorkAttend.Model.Models.Admin
{
    public class CreateAdminRequest
    {
        public string adminEmail { get; set; } = string.Empty;
        public string adminName { get; set; } = string.Empty;
        public int roleID { get; set; }
    }

    public class AdminsIndexResponse
    {
        public List<Roles> roles { get; set; } = new();
        public List<adminMod> adminList { get; set; } = new();
    }

    public class RoleOverviewDto
    {
        public int roleID { get; set; }
        public string policy { get; set; } = string.Empty;
        public string roleName { get; set; } = string.Empty;
        public string roleDescription { get; set; } = string.Empty;
        public int numberOfAdmins { get; set; }
    }

    public class RolesOverviewResponse
    {
        public List<RoleOverviewDto> roleDataList { get; set; } = new();
        public List<permission> permissions { get; set; } = new();
    }
}
