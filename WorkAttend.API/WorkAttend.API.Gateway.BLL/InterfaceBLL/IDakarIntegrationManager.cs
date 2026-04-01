using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IDakarIntegrationManager
    {
        Task<ApiResponse<dakarIntegrationMod>> GetConfigAsync(CurrentUserContext ctx);
        Task<ApiResponse<bool>> CreateConfigAsync(CurrentUserContext ctx, dakarIntegrationMod model);
        Task<ApiResponse<bool>> UpdateConfigAsync(CurrentUserContext ctx, dakarIntegrationMod model);
        Task<ApiResponse<bool>> DeleteConfigAsync(CurrentUserContext ctx, int companyConfigId);
    }
}