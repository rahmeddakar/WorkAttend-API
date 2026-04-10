using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface ISubscriptionManager
    {
        Task<ApiResponse<subscription>> GetPageDataAsync(CurrentUserContext ctx);
        Task<ApiResponse<bool>> ToggleSubscriptionFeatureAsync(CurrentUserContext ctx, ConfigurableFeature model);
        Task<ApiResponse<SiteTokenModel>> SubscribePlanAsync(CurrentUserContext ctx, subscribePlan model);
        Task<ApiResponse<bool>> AddAddressAsync(CurrentUserContext ctx, billingAddress model);

        Task<string> ConfirmPaymentStatusAsync(string userId, string token);
        Task<string> GetPaymentRedirectUrlAsync(string userId, string token);
    }
}