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
    public class ActivityController : ControllerBase
    {
        private readonly IActivityManager _activityManager;

        public ActivityController(IActivityManager activityManager)
        {
            _activityManager = activityManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                AppLogger.Info(
                    message: "Get activities request received",
                    action: "View",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: string.Empty);

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Get activities request unauthorized because token context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: "CurrentUserContext was null");

                    return Unauthorized(new ApiResponse<System.Collections.Generic.List<Activity>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _activityManager.GetActivitiesAsync(ctx);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Get activities request completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"ActivitiesCount={response.Data?.Count ?? 0}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Get activities request failed with unexpected exception",
                    action: "View",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<System.Collections.Generic.List<Activity>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [HttpPost("save-activity")]
        public async Task<IActionResult> SaveActivity([FromBody] projectClientList model)
        {
            try
            {
                AppLogger.Info(
                    message: "Save activity request received",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"ActivityId={model?.activityID}, ActivityName={model?.name}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Save activity request unauthorized because token context was not found",
                        action: model?.isForAdd == true ? "Create" : "Update",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"ActivityId={model?.activityID}, ActivityName={model?.name}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _activityManager.SaveActivityAsync(ctx, model);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Save activity request completed successfully",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"ActivityId={model?.activityID}, ActivityName={model?.name}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Save activity request failed with unexpected exception",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"ActivityId={model?.activityID}, ActivityName={model?.name}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                });
            }
        }

        [HttpDelete("{activityId:int}")]
        public async Task<IActionResult> DeleteActivity(int activityId)
        {
            try
            {
                AppLogger.Info(
                    message: "Delete activity request received",
                    action: "Delete",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"ActivityId={activityId}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Delete activity request unauthorized because token context was not found",
                        action: "Delete",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"ActivityId={activityId}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _activityManager.DeleteActivityAsync(ctx, activityId);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Delete activity request completed successfully",
                    action: "Delete",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"ActivityId={activityId}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Delete activity request failed with unexpected exception",
                    action: "Delete",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"ActivityId={activityId}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                });
            }
        }
    }
}