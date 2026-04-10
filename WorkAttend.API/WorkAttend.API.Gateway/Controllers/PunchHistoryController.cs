using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.CommonCode.Helpers;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PunchHistoryController : ControllerBase
    {
        private readonly IPunchHistoryManager _punchHistoryManager;

        public PunchHistoryController(IPunchHistoryManager punchHistoryManager)
        {
            _punchHistoryManager = punchHistoryManager;
        }

        [HttpPost("page-data")]
        public async Task<IActionResult> GetPunchHistoryPageData([FromBody] Punch model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<Punch>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _punchHistoryManager.GetPunchHistoryPageDataAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("filtered-history")]
        public async Task<IActionResult> GetFilteredHistory([FromBody] Punch model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<Punch>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _punchHistoryManager.GetFilteredHistoryAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("manual-punches")]
        public async Task<IActionResult> GetFilteredManualPunches([FromBody] ManualPunch model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<ManualPunch>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _punchHistoryManager.GetFilteredManualPunchesAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("create-page-data")]
        public async Task<IActionResult> GetCreatePunchPageData([FromBody] createPunch model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<createPunch>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _punchHistoryManager.GetCreatePunchPageDataAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePunch([FromBody] createPunch model)
        {
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

            var response = await _punchHistoryManager.CreatePunchAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportPunchHistory([FromBody] PunchHistoryExportRequest model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _punchHistoryManager.ExportPunchHistoryAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            if (response.Data == null || !response.Data.HasData || response.Data.FileBytes == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = response.Data?.Message ?? "No data is available.",
                    Data = null
                });
            }

            return File(response.Data.FileBytes, response.Data.ContentType, response.Data.FileName);
        }

        [HttpPut("approve-reject-manual-punch")]
        public async Task<IActionResult> ApproveRejectManualPunches([FromBody] ManualPunchApprovalRequest model)
        {
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

            var response = await _punchHistoryManager.ApproveRejectManualPunchesAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}