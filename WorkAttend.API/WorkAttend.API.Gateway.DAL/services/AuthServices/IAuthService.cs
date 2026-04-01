using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.AuthServices
{
    public interface IAuthService
    {
        Task<Companyconfigurations?> GetCompanyConfigurationAsync(string companyURL);
        Task<bool> GetActivePackageAsync(int companyId);
        Task<workattendadmin?> GetAdminFromEmailAsync(string email, string databaseName);
        Task<workattendadmin?> GetAdminByIdAsync(int adminId, string databaseName);
        Task<bool> IsRedundantRequestAsync(int adminId, string databaseName);
        Task<adminresettoken?> StorePasswordResetTokenAsync(string adminEmail, int adminId, string token, string databaseName);
        Task<adminresettoken?> CheckTokenExistAsync(int adminId, string token, string databaseName);
        Task<List<int>> GetAdminTokensAsync(string databaseName, int adminId);
        Task<bool> ExpireAllTokensAsync(string databaseName, List<int> tokenIds, int adminId);
        Task<bool> UpdateAdminPasswordAsync(int adminId, string password, string databaseName);
        Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage);
    }
}