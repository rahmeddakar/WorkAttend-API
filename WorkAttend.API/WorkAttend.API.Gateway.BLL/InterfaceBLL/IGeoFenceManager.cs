using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IGeoFenceManager
    {
        Task<ApiResponse<List<Location>>> GetGeoFencesAsync(CurrentUserContext ctx);
        Task<ApiResponse<bool>> CreateGeoFenceAsync(CurrentUserContext ctx, Location model);
        Task<ApiResponse<bool>> DeleteGeoFenceAsync(CurrentUserContext ctx, int id);
        Task<ApiResponse<bool>> EditGeoFenceAsync(CurrentUserContext ctx, Location model);
    }
}