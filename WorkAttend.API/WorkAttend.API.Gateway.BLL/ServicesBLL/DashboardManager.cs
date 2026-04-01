using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.services.DashboardServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class DashboardManager : IDashboardManager
    {
        private readonly IDashboardService _dashboardService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public DashboardManager(
            IDashboardService dashboardService,
            IUserAccessContextManager userAccessContextManager)
        {
            _dashboardService = dashboardService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<DashboardStats>> GetStatsAsync(CurrentUserContext ctx, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                AppLogger.Info(
                    message: "Dashboard stats load started",
                    action: "View",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"StartDate={startDate:O}, EndDate={endDate:O}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Dashboard stats load blocked because access context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"StartDate={startDate:O}, EndDate={endDate:O}");

                    return new ApiResponse<DashboardStats>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "dashboard");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Dashboard stats load blocked by permission",
                        action: "View",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow view on dashboard");

                    return new ApiResponse<DashboardStats>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var normalized = NormalizeDates(startDate, endDate);

                int totalEmployees = await _dashboardService.GetEmployeesCountAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                List<employeepunchhistory> punchHistory = await _dashboardService.GetPunchesCountAsync(
                    accessContext.CompanyId,
                    normalized.startDate,
                    normalized.endDate,
                    accessContext.DatabaseName);

                List<int> inEmployeesList = punchHistory
                    .Where(p => p.punchType == 1)
                    .Select(p => p.employeeID)
                    .Distinct()
                    .ToList();

                List<int> outEmployeesList = punchHistory
                    .Where(p => p.punchType == 2)
                    .Select(p => p.employeeID)
                    .Distinct()
                    .ToList();

                List<int> onSiteEmployeeList = inEmployeesList.Except(outEmployeesList).ToList();

                List<resultsQuest> results = await _dashboardService.GetResultsAsync(
                    accessContext.CompanyId,
                    normalized.startDate,
                    normalized.endDate,
                    accessContext.DatabaseName);

                int lowCount = results.Count(p =>
                    p.questionaireScaleID == 1 ||
                    string.Equals(p.rangeText, "low", StringComparison.OrdinalIgnoreCase));

                int mediumCount = results.Count(p =>
                    p.questionaireScaleID == 2 ||
                    string.Equals(p.rangeText, "medium", StringComparison.OrdinalIgnoreCase));

                int highCount = results.Count(p =>
                    p.questionaireScaleID == 3 ||
                    string.Equals(p.rangeText, "high", StringComparison.OrdinalIgnoreCase));

                DashboardStats stats = new DashboardStats
                {
                    totalEmployees = totalEmployees,
                    totalIn = inEmployeesList.Count,
                    totalOut = outEmployeesList.Count,
                    onSite = onSiteEmployeeList.Count,
                    lowRisk = lowCount,
                    mediumRisk = mediumCount,
                    highRisk = highCount,
                    resultList = results,
                    startDate = normalized.startDate,
                    endDate = normalized.endDate,
                    dateFilter = $"{normalized.startDate:MM/dd/yyyy} - {normalized.endDate:MM/dd/yyyy}"
                };

                AppLogger.Info(
                    message: "Dashboard stats load completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"TotalEmployees={stats.totalEmployees}, TotalIn={stats.totalIn}, TotalOut={stats.totalOut}, OnSite={stats.onSite}, Results={results.Count}");

                return new ApiResponse<DashboardStats>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = stats
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Dashboard stats load failed",
                    action: "View",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"StartDate={startDate:O}, EndDate={endDate:O}",
                    exception: ex);

                await _dashboardService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "DashboardManager.GetStatsAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<DashboardStats>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        private async Task<UserAccessContext?> ResolveAccessContextAsync(CurrentUserContext ctx)
        {
            return await _userAccessContextManager.GetAsync(ctx);
        }

        private static (DateTime startDate, DateTime endDate) NormalizeDates(DateTime? startDate, DateTime? endDate)
        {
            DateTime finalStart;
            DateTime finalEnd;

            if (!startDate.HasValue || !endDate.HasValue || startDate.Value == DateTime.MinValue || endDate.Value == DateTime.MinValue)
            {
                DateTime now = DateTime.Now;
                finalStart = now.Date.Add(new TimeSpan(0, 0, 0));
                finalEnd = now.Date.Add(new TimeSpan(23, 59, 59));
            }
            else
            {
                finalStart = startDate.Value.Date.Add(new TimeSpan(0, 0, 0));
                finalEnd = endDate.Value.Date.Add(new TimeSpan(23, 59, 59));
            }

            return (finalStart, finalEnd);
        }
    }
}