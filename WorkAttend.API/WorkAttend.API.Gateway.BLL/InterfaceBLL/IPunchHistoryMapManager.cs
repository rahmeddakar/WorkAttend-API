using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IPunchHistoryMapManager
    {
        Task<ApiResponse<punchHistoryMap>> GetPageDataAsync(CurrentUserContext ctx, punchHistoryMap model);
        Task<ApiResponse<punchHistoryMap>> GetFilterDataAsync(CurrentUserContext ctx, punchHistoryMap model);
    }
}