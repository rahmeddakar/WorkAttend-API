using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IActivityManager
    {
        Task<ApiResponse<List<Activity>>> GetActivitiesAsync(CurrentUserContext ctx);
        Task<ApiResponse<bool>> SaveActivityAsync(CurrentUserContext ctx, projectClientList model);
        Task<ApiResponse<bool>> DeleteActivityAsync(CurrentUserContext ctx, int activityId);
    }
}