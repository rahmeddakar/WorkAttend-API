using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkAttend.API.Gateway.BLL.CommonCode.Helpers;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeManager _homeManager;

        public HomeController(IHomeManager homeManager)
        {
            _homeManager = homeManager;
        }

        [HttpGet("qr-code")]
        public async Task<IActionResult> GetQrCode()
        {
            try
            {
                AppLogger.Info(
                    message: "QR code request received",
                    action: "View",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: string.Empty);

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _homeManager.GetQrCodeAsync(ctx);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(new ApiResponse<object> { Success = false, Message = response.Message, Data = null });

                    return BadRequest(new ApiResponse<object> { Success = false, Message = response.Message, Data = null });
                }

                return File(response.Data, "image/png");
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "QR code request failed with unexpected exception",
                    action: "View",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [HttpGet("emergency-list/pdf")]
        public async Task<IActionResult> CreateEmergencyListPdf()
        {
            try
            {
                AppLogger.Info(
                    message: "Emergency list PDF request received",
                    action: "Export",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: string.Empty);

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _homeManager.CreateEmergencyListPdfAsync(ctx);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(new ApiResponse<object> { Success = false, Message = response.Message, Data = null });

                    return BadRequest(new ApiResponse<object> { Success = false, Message = response.Message, Data = null });
                }

                return File(response.Data, "application/pdf", "emergencyList.pdf");
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Emergency list PDF request failed with unexpected exception",
                    action: "Export",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }
    }
}