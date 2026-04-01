using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardManager _dashboardManager;

        public DashboardController(IDashboardManager dashboardManager)
        {
            _dashboardManager = dashboardManager;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                AppLogger.Info(
                    message: "Dashboard stats request received",
                    action: "View",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"StartDate={startDate:O}, EndDate={endDate:O}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Dashboard stats request unauthorized because token context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"StartDate={startDate:O}, EndDate={endDate:O}");

                    return Unauthorized(new ApiResponse<DashboardStats>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _dashboardManager.GetStatsAsync(ctx, startDate, endDate);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Dashboard stats request completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"StartDate={startDate:O}, EndDate={endDate:O}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Dashboard stats request failed with unexpected exception",
                    action: "View",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"StartDate={startDate:O}, EndDate={endDate:O}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<DashboardStats>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }
    }
}