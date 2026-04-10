using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.HomeServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class HomeManager : IHomeManager
    {
        private readonly IHomeService _homeService;
        private readonly HomeDataHelper _homeDataHelper;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public HomeManager(
            IHomeService homeService,
            HomeDataHelper homeDataHelper,
            IUserAccessContextManager userAccessContextManager)
        {
            _homeService = homeService;
            _homeDataHelper = homeDataHelper;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<byte[]>> GetQrCodeAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "QR code generation started",
                    action: "View",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"CompanyURL={ctx.CompanyURL}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "QR code generation blocked because access context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"CompanyURL={ctx.CompanyURL}");

                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "home");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "QR code generation blocked by permission",
                        action: "View",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow view on home");

                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var companyConfig = await _homeDataHelper.GetCompanyConfigurationsAsync(ctx.CompanyURL);
                string qrText = companyConfig?.mobileApplicationKey ?? string.Empty;

                if (string.IsNullOrWhiteSpace(qrText))
                {
                    AppLogger.Warn(
                        message: "QR code generation failed because company mobile application key was not found",
                        action: "View",
                        result: "NotFound",
                        updatedBy: accessContext.UserId,
                        description: $"CompanyURL={ctx.CompanyURL}");

                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "QR code data not found.",
                        Data = null
                    };
                }

                byte[] imageBytes = await _homeService.GenerateQrCodeAsync(qrText);

                AppLogger.Info(
                    message: "QR code generation completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"CompanyURL={ctx.CompanyURL}");

                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = imageBytes
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "QR code generation failed",
                    action: "View",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"CompanyURL={ctx.CompanyURL}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "HomeManager.GetQrCodeAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<byte[]>> CreateEmergencyListPdfAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "Emergency list PDF generation started",
                    action: "Export",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Emergency list PDF generation blocked because access context was not found",
                        action: "Export",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "home");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Export.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Emergency list PDF generation blocked by permission",
                        action: "Export",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow export on home");

                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                DateTime now = DateTime.Now;
                DateTime startDate = now.Date.Add(new TimeSpan(0, 0, 0));
                DateTime endDate = now.Date.Add(new TimeSpan(23, 59, 59));

                var onSiteEmployeeIds = await _homeDataHelper.GetEmergencyOnSiteEmployeeIdsAsync(
                    accessContext.CompanyId,
                    startDate,
                    endDate,
                    accessContext.DatabaseName);

                if (onSiteEmployeeIds == null || onSiteEmployeeIds.Count == 0)
                {
                    AppLogger.Warn(
                        message: "Emergency list PDF generation found no on-site employees",
                        action: "Export",
                        result: "NoData",
                        updatedBy: accessContext.UserId,
                        description: $"CompanyId={accessContext.CompanyId}");

                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "No emergency data found.",
                        Data = null
                    };
                }

                int mobileAttrId = await _homeDataHelper.GetAttributeIdAsync("MobileNumber", accessContext.DatabaseName);
                int punchAttrId = await _homeDataHelper.GetPunchAttributeIdAsync("assemblypoint", accessContext.DatabaseName, accessContext.CompanyId);

                var latestPunches = await _homeDataHelper.GetEmergencyListPunchesAsync(
                    onSiteEmployeeIds,
                    mobileAttrId,
                    punchAttrId,
                    accessContext.CompanyId,
                    startDate,
                    endDate,
                    accessContext.DatabaseName);

                if (latestPunches == null || latestPunches.Count == 0)
                {
                    AppLogger.Warn(
                        message: "Emergency list PDF generation found no emergency punch records",
                        action: "Export",
                        result: "NoData",
                        updatedBy: accessContext.UserId,
                        description: $"CompanyId={accessContext.CompanyId}");

                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "No emergency data found.",
                        Data = null
                    };
                }

                var assemblyPointIds = latestPunches
                    .Select(x => x.assemblypointID)
                    .Distinct()
                    .ToList();

                List<emergencyList> emergencyLists = new List<emergencyList>();

                foreach (var assemblyPointId in assemblyPointIds)
                {
                    var assemblyData = latestPunches
                        .Where(x => x.assemblypointID == assemblyPointId)
                        .ToList();

                    if (assemblyData.Count == 0)
                        continue;

                    emergencyLists.Add(new emergencyList
                    {
                        emergencyData = assemblyData,
                        assemblyPointID = assemblyPointId,
                        printedOn = DateTime.Now,
                        assemblyPoint = assemblyData[0].assemblypoint
                    });
                }

                if (emergencyLists.Count == 0)
                {
                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "No emergency data found.",
                        Data = null
                    };
                }

                byte[] pdfBytes = await _homeService.GenerateEmergencyListPdfAsync(emergencyLists);

                AppLogger.Info(
                    message: "Emergency list PDF generation completed successfully",
                    action: "Export",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"CompanyId={accessContext.CompanyId}, Groups={emergencyLists.Count}");

                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = pdfBytes
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Emergency list PDF generation failed",
                    action: "Export",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "HomeManager.CreateEmergencyListPdfAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<byte[]>
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