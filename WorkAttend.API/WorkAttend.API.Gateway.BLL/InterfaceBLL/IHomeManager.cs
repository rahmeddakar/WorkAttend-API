using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IHomeManager
    {
        Task<ApiResponse<byte[]>> GetQrCodeAsync(CurrentUserContext ctx);
        Task<ApiResponse<byte[]>> CreateEmergencyListPdfAsync(CurrentUserContext ctx);
    }
}