using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface ICompanyManager
    {
        Task<ApiResponse<RegisterCompanyPageData>> GetRegisterDataAsync();
        Task<ApiResponse<CompanyRegistrationCheckResponse>> CompanyURLExistAsync(CompanyRegistrationCheckRequest model);
        Task<ApiResponse<RegisterCompanyResult>> RegisterCompanyAsync(registerCompany registerModel);
    }
}