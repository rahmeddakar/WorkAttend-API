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
    public class EmployeeProjHistoryController : ControllerBase
    {
        private readonly IEmployeeProjHistoryManager _employeeProjHistoryManager;

        public EmployeeProjHistoryController(IEmployeeProjHistoryManager employeeProjHistoryManager)
        {
            _employeeProjHistoryManager = employeeProjHistoryManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                AppLogger.Info(
                    message: "Employee project history page data request received",
                    action: "View",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: string.Empty);

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Employee project history page data request unauthorized because token context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: string.Empty);

                    return Unauthorized(new ApiResponse<Punch>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _employeeProjHistoryManager.GetPageDataAsync(ctx);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Employee project history page data request completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"Employees={response.Data?.employees?.Count ?? 0}, Locations={response.Data?.locations?.Count ?? 0}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Employee project history page data request failed with unexpected exception",
                    action: "View",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<Punch>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }
    }
}