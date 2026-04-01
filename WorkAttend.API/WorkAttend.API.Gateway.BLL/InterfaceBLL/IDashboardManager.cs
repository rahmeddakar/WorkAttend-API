using System;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IDashboardManager
    {
        Task<ApiResponse<DashboardStats>> GetStatsAsync(CurrentUserContext ctx, DateTime? startDate, DateTime? endDate);
    }
}