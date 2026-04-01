using System.Threading.Tasks;
using WorkAttend.Model.Models;
using WorkAttend.Model.Models.Auth;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IAuthManager
    {
        Task<ApiResponse<bool>> CompanyExistsAsync(CompanyExistsRequest request);
        Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ApiResponse<bool>> ValidateResetTokenAsync(ValidateResetTokenRequest request);
        Task<ApiResponse<bool>> UpdatePasswordAsync(UpdatePasswordRequest request);
    }
}