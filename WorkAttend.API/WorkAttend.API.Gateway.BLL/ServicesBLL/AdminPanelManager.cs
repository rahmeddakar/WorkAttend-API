using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.services.AdminPanelServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class AdminPanelManager : IAdminPanelManager
    {
        private readonly IAdminPanelService _adminPanelService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public AdminPanelManager(
            IAdminPanelService adminPanelService,
            IUserAccessContextManager userAccessContextManager)
        {
            _adminPanelService = adminPanelService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<List<AdminPanelItem>>> GetDataAsync(
            CurrentUserContext ctx,
            bool? isDakarConnected = true,
            string companyName = "",
            int packageId = 0,
            int limit = 50)
        {
            try
            {
                AppLogger.Info(
                    message: "Admin panel data load started",
                    action: "View",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"IsDakarConnected={isDakarConnected}, CompanyName={companyName}, PackageId={packageId}, Limit={limit}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Admin panel data load blocked because access context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: string.Empty);

                    return new ApiResponse<List<AdminPanelItem>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "adminpanel");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Admin panel data load blocked by permission",
                        action: "View",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow view on adminpanel");

                    return new ApiResponse<List<AdminPanelItem>>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var data = await _adminPanelService.GetAdminPanelItemsAsync(isDakarConnected, companyName, packageId, limit);

                AppLogger.Info(
                    message: "Admin panel data load completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"Count={data.Count}");

                return new ApiResponse<List<AdminPanelItem>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Admin panel data load failed",
                    action: "View",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"IsDakarConnected={isDakarConnected}, CompanyName={companyName}, PackageId={packageId}, Limit={limit}",
                    exception: ex);

                await _adminPanelService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminPanelManager.GetDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<List<AdminPanelItem>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<List<subscriptionpackage>>> GetPackagesAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "Admin panel packages load started",
                    action: "View",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: string.Empty);

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Admin panel packages load blocked because access context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: string.Empty);

                    return new ApiResponse<List<subscriptionpackage>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "adminpanel");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Admin panel packages load blocked by permission",
                        action: "View",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow view on adminpanel");

                    return new ApiResponse<List<subscriptionpackage>>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var packages = await _adminPanelService.GetSubscriptionPackagesAsync();

                AppLogger.Info(
                    message: "Admin panel packages load completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"Count={packages.Count}");

                return new ApiResponse<List<subscriptionpackage>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = packages
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Admin panel packages load failed",
                    action: "View",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: string.Empty,
                    exception: ex);

                await _adminPanelService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminPanelManager.GetPackagesAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<List<subscriptionpackage>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<AdminPanelItem>> GetCompanyDetailAsync(CurrentUserContext ctx, int companyId)
        {
            try
            {
                AppLogger.Info(
                    message: "Admin panel company detail load started",
                    action: "View",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"CompanyId={companyId}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Admin panel company detail load blocked because access context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"CompanyId={companyId}");

                    return new ApiResponse<AdminPanelItem>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "adminpanel");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Admin panel company detail load blocked by permission",
                        action: "View",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"CompanyId={companyId}");

                    return new ApiResponse<AdminPanelItem>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                if (companyId <= 0)
                {
                    AppLogger.Warn(
                        message: "Admin panel company detail request validation failed",
                        action: "View",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: $"CompanyId={companyId}");

                    return new ApiResponse<AdminPanelItem>
                    {
                        Success = false,
                        Message = "Invalid company id.",
                        Data = null
                    };
                }

                var data = await _adminPanelService.GetCompanyDetailAsync(companyId);

                if (data == null)
                {
                    AppLogger.Warn(
                        message: "Admin panel company detail was not found",
                        action: "View",
                        result: "NotFound",
                        updatedBy: accessContext.UserId,
                        description: $"CompanyId={companyId}");

                    return new ApiResponse<AdminPanelItem>
                    {
                        Success = false,
                        Message = "Company detail not found.",
                        Data = null
                    };
                }

                AppLogger.Info(
                    message: "Admin panel company detail load completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"CompanyId={companyId}");

                return new ApiResponse<AdminPanelItem>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Admin panel company detail load failed",
                    action: "View",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"CompanyId={companyId}",
                    exception: ex);

                await _adminPanelService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminPanelManager.GetCompanyDetailAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<AdminPanelItem>
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