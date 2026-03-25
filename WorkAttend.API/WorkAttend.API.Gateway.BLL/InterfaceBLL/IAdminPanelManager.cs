using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IAdminPanelManager
    {
        Task<ApiResponse<List<AdminPanelItem>>> GetDataAsync(
            CurrentUserContext ctx,
            bool? isDakarConnected = true,
            string companyName = "",
            int packageId = 0,
            int limit = 50);

        Task<ApiResponse<List<subscriptionpackage>>> GetPackagesAsync(CurrentUserContext ctx);

        Task<ApiResponse<AdminPanelItem>> GetCompanyDetailAsync(CurrentUserContext ctx, int companyId);
    }
}