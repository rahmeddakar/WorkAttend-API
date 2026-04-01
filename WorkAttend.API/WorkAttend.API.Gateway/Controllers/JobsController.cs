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
    public class JobsController : ControllerBase
    {
        private readonly IJobsManager _jobsManager;

        public JobsController(IJobsManager jobsManager)
        {
            _jobsManager = jobsManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                AppLogger.Info(
                    message: "Jobs request received",
                    action: "View",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: string.Empty);

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    return Unauthorized(new ApiResponse<List<Jobs>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _jobsManager.GetJobsAsync(ctx);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Jobs request completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"Count={response.Data?.Count ?? 0}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Jobs request failed with unexpected exception",
                    action: "View",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<Jobs>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [HttpPost("save-job")]
        public async Task<IActionResult> SaveJob([FromBody] JobViewModel model)
        {
            try
            {
                AppLogger.Info(
                    message: "Save job request received",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"JobId={model?.JobID}, JobName={model?.name}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _jobsManager.SaveJobAsync(ctx, model);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Save job request completed successfully",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"JobId={model?.JobID}, JobName={model?.name}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Save job request failed with unexpected exception",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"JobId={model?.JobID}, JobName={model?.name}",
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