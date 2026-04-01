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
    public class GeoFenceController : ControllerBase
    {
        private readonly IGeoFenceManager _geoFenceManager;

        public GeoFenceController(IGeoFenceManager geoFenceManager)
        {
            _geoFenceManager = geoFenceManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                AppLogger.Info(
                    message: "Geo-fence list request received",
                    action: "View",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: string.Empty);

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Geo-fence list request unauthorized because token context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: string.Empty);

                    return Unauthorized(new ApiResponse<List<Location>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _geoFenceManager.GetGeoFencesAsync(ctx);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Geo-fence list request completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"Count={response.Data?.Count ?? 0}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Geo-fence list request failed with unexpected exception",
                    action: "View",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<Location>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGeoFence([FromBody] Location model)
        {
            try
            {
                AppLogger.Info(
                    message: "Create geo-fence request received",
                    action: "Create",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"LocationName={model?.LocationName}, LocationCode={model?.LocationCode}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Create geo-fence request unauthorized because token context was not found",
                        action: "Create",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"LocationName={model?.LocationName}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _geoFenceManager.CreateGeoFenceAsync(ctx, model);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Create geo-fence request completed successfully",
                    action: "Create",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"LocationName={model?.LocationName}, LocationCode={model?.LocationCode}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Create geo-fence request failed with unexpected exception",
                    action: "Create",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"LocationName={model?.LocationName}, LocationCode={model?.LocationCode}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                AppLogger.Info(
                    message: "Delete geo-fence request received",
                    action: "Delete",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"LocationId={id}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Delete geo-fence request unauthorized because token context was not found",
                        action: "Delete",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"LocationId={id}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _geoFenceManager.DeleteGeoFenceAsync(ctx, id);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Delete geo-fence request completed successfully",
                    action: "Delete",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"LocationId={id}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Delete geo-fence request failed with unexpected exception",
                    action: "Delete",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"LocationId={id}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditLocation([FromBody] Location model)
        {
            try
            {
                AppLogger.Info(
                    message: "Edit geo-fence request received",
                    action: "Update",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"LocationId={model?.Id}, LocationName={model?.LocationName}, LocationCode={model?.LocationCode}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Edit geo-fence request unauthorized because token context was not found",
                        action: "Update",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"LocationId={model?.Id}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _geoFenceManager.EditGeoFenceAsync(ctx, model);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Edit geo-fence request completed successfully",
                    action: "Update",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"LocationId={model?.Id}, LocationName={model?.LocationName}, LocationCode={model?.LocationCode}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Edit geo-fence request failed with unexpected exception",
                    action: "Update",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"LocationId={model?.Id}",
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