using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;
using WorkAttend.Model.Models.Admin;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IAdminsManager
    {
        Task<string?> GetDatabaseNameByCompanyUrlAsync(string companyUrl);
        Task<workattendadmin?> ValidateAdminAsync(string databaseName, string email, string password);
        Task<UserAccessContext?> GetUserAccessContextAsync(string userId, string databaseName, string companyURL, string email);

        Task<AdminsIndexResponse> GetAdminsIndexDataAsync(CurrentUserContext ctx);
        Task<RolesOverviewResponse> GetRolesOverviewAsync(CurrentUserContext ctx);
        Task<List<Roles>> GetRolesOnlyAsync(CurrentUserContext ctx);

        Task<ApiResponse<object>> CreateRoleAsync(CurrentUserContext ctx, rolesDataModel model);
        Task<ApiResponse<object>> CreateAdminAsync(CurrentUserContext ctx, CreateAdminRequest model);
        Task<ApiResponse<bool>> DeleteAdminAsync(CurrentUserContext ctx, int adminId);
    }
}