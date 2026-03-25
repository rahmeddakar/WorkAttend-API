using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.AdminPanelServices
{
    public interface IAdminPanelService
    {
        Task<List<AdminPanelItem>> GetAdminPanelItemsAsync(bool? isDakarConnected, string companyName, int packageId, int limit);
        Task<List<subscriptionpackage>> GetSubscriptionPackagesAsync();
        Task<AdminPanelItem?> GetCompanyDetailAsync(int companyId);
        Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage);
    }
}