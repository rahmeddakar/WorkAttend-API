using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.CommonCode.Helpers;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminPanelController : ControllerBase
    {
        private readonly IAdminPanelManager _adminPanelManager;

        public AdminPanelController(IAdminPanelManager adminPanelManager)
        {
            _adminPanelManager = adminPanelManager;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetData(
            bool? isDakarConnected = true,
            string companyName = "",
            int packageId = 0,
            int limit = 50)
        {
            try
            {
                AppLogger.Info(
                    message: "Admin panel data request received",
                    action: "View",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"IsDakarConnected={isDakarConnected}, CompanyName={companyName}, PackageId={packageId}, Limit={limit}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Admin panel data request unauthorized because token context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: string.Empty);

                    return Unauthorized(new ApiResponse<List<AdminPanelItem>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _adminPanelManager.GetDataAsync(ctx, isDakarConnected, companyName, packageId, limit);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Admin panel data request completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"Count={response.Data?.Count ?? 0}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Admin panel data request failed with unexpected exception",
                    action: "View",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"IsDakarConnected={isDakarConnected}, CompanyName={companyName}, PackageId={packageId}, Limit={limit}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<AdminPanelItem>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            try
            {
                AppLogger.Info(
                    message: "Admin panel packages request received",
                    action: "View",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: string.Empty);

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Admin panel packages request unauthorized because token context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: string.Empty);

                    return Unauthorized(new ApiResponse<List<subscriptionpackage>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _adminPanelManager.GetPackagesAsync(ctx);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Admin panel packages request completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"Count={response.Data?.Count ?? 0}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Admin panel packages request failed with unexpected exception",
                    action: "View",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<subscriptionpackage>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [HttpGet("company-detail/{companyId:int}")]
        public async Task<IActionResult> CompanyDetail(int companyId)
        {
            try
            {
                AppLogger.Info(
                    message: "Admin panel company detail request received",
                    action: "View",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Admin panel company detail request unauthorized because token context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"CompanyId={companyId}");

                    return Unauthorized(new ApiResponse<AdminPanelItem>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _adminPanelManager.GetCompanyDetailAsync(ctx, companyId);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Admin panel company detail request completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"CompanyId={companyId}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Admin panel company detail request failed with unexpected exception",
                    action: "View",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<AdminPanelItem>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }
    }
}