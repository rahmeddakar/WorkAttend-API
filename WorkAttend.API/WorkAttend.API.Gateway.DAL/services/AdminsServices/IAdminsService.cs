using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.AdminsServices
{
    public interface IAdminsService
    {
        Task<string?> GetDatabaseNameByCompanyUrlAsync(string companyUrl);
        Task<workattendadmin?> ValidateAdminAsync(string databaseName, string email, string encryptedPassword);

        Task<List<Roles>> GetRoles(int companyId, string databaseName);
        Task<List<adminMod>> GetAllAdmins(int companyId, string databaseName);
        Task<List<permission>> GetPermissions(string databaseName);
        Task<int> GetRolesCount(int companyId, int roleId, string databaseName);

        Task<Roles?> AddRole(int companyId, string policy, string databaseName, string roleName, string roleDescription, string userId);

        Task<subscriptionpackagefeatures?> GetActivePackageData(int companyId, int featureId = 0);
        Task<List<SubscriptionPackageFeatureModel>> GetCompanySubscriptionFeatures(int companyId, int featureId = 0);
        Task<int> GetDatabaseAdminCount(string databaseName);

        Task<workattendadmin?> GetAdminFromEmail(string email, string databaseName);
        Task<companyadmin?> CheckAdminCompanyExist(int adminId, int companyId, string databaseName);
        Task<companyadmin> AddCompanyAdmin(int adminId, int companyId, string databaseName);
        Task<workattendadmin> AddAdmin(string adminEmail, string password, string userId, string databaseName, string name);
        Task<adminroles> AddAdminRole(int adminId, int roleId, string userId, string databaseName, int companyId);
        Task<bool> DeleteAdmin(int adminId, string userId, int companyId, string databaseName);
        Task<UserAccessContext?> GetUserAccessContextAsync(string userId, string databaseName);
    }
}
