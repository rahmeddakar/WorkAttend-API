using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IJobsManager
    {
        Task<ApiResponse<List<Jobs>>> GetJobsAsync(CurrentUserContext ctx);
        Task<ApiResponse<bool>> SaveJobAsync(CurrentUserContext ctx, JobViewModel model);
    }
}