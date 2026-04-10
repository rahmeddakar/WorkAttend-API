using System;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.EmployeeProjHistoryServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class EmployeeProjHistoryManager : IEmployeeProjHistoryManager
    {
        private const int DefaultDepartmentId = 1;

        private readonly IEmployeeProjHistoryService _employeeProjHistoryService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public EmployeeProjHistoryManager(
            IEmployeeProjHistoryService employeeProjHistoryService,
            IUserAccessContextManager userAccessContextManager)
        {
            _employeeProjHistoryService = employeeProjHistoryService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<Punch>> GetPageDataAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "Employee project history page data load started",
                    action: "View",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Employee project history page data load blocked because access context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new ApiResponse<Punch>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                int departmentId = DefaultDepartmentId;

                var employees = await _employeeProjHistoryService.GetEmployeesAsync(
                    accessContext.CompanyId,
                    departmentId,
                    accessContext.DatabaseName);

                var locations = await _employeeProjHistoryService.GetLocationsAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                var model = new Punch
                {
                    employees = employees,
                    locations = locations
                };

                AppLogger.Info(
                    message: "Employee project history page data load completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"CompanyId={accessContext.CompanyId}, DepartmentId={departmentId}, Employees={employees.Count}, Locations={locations.Count}");

                return new ApiResponse<Punch>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = model
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Employee project history page data load failed",
                    action: "View",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeProjHistoryManager.GetPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<Punch>
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
    }
}