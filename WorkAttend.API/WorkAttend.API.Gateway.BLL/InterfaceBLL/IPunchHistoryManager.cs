using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IPunchHistoryManager
    {
        Task<ApiResponse<Punch>> GetPunchHistoryPageDataAsync(CurrentUserContext ctx, Punch model);
        Task<ApiResponse<Punch>> GetFilteredHistoryAsync(CurrentUserContext ctx, Punch model);
        Task<ApiResponse<ManualPunch>> GetFilteredManualPunchesAsync(CurrentUserContext ctx, ManualPunch model);
        Task<ApiResponse<createPunch>> GetCreatePunchPageDataAsync(CurrentUserContext ctx, createPunch model);
        Task<ApiResponse<bool>> CreatePunchAsync(CurrentUserContext ctx, createPunch model);
        Task<ApiResponse<PunchHistoryExportFile>> ExportPunchHistoryAsync(CurrentUserContext ctx, PunchHistoryExportRequest model);
        Task<ApiResponse<bool>> ApproveRejectManualPunchesAsync(CurrentUserContext ctx, ManualPunchApprovalRequest model);
    }
}