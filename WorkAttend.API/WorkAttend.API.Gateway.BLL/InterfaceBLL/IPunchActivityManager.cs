using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IPunchActivityManager
    {
        Task<ApiResponse<punchActivityListModel>> GetPunchActivitiesAsync(CurrentUserContext ctx);
        Task<ApiResponse<bool>> SavePunchActivityAsync(CurrentUserContext ctx, punchActivityModel model);
        Task<ApiResponse<bool>> EditPunchActivityAsync(CurrentUserContext ctx, editPunchModel model);
    }
}