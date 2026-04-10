using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IProjectManager
    {
        Task<ApiResponse<projectMod>> GetProjectsPageDataAsync(CurrentUserContext ctx);
        Task<ApiResponse<projectClientList>> SaveProjectAsync(CurrentUserContext ctx, projectClientList model);
        Task<ApiResponse<bool>> DeleteProjectAsync(CurrentUserContext ctx, int projectId);
    }
}