using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.DakarIntegrationServices
{
    public interface IDakarIntegrationService
    {
        Task<dakarcompanyconfigs?> GetDakarConfigAsync(int baseCompanyId);
        Task<dakarcompanyconfigs> CreateDakarCompanyConfigAsync(int baseCompanyId, string userId, string dakarURL, string companyCode, string siteCode);
        Task<bool> UpdateDakarConnectedAsync(int companyId, string databaseName, string userId);
        Task<bool> UpdateDakarConnectedBaseAsync(int baseCompanyId, string userId);
        Task<bool> UpdateDakarURLAsync(int companyConfigId, string dakarURL, string companyCode, string siteCode, string userId);
        Task<bool> DeleteDakarURLAsync(int companyConfigId, string userId);
    }
}