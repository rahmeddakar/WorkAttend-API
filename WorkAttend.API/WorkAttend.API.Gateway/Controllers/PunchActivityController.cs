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
    public class PunchActivityController : ControllerBase
    {
        private readonly IPunchActivityManager _punchActivityManager;

        public PunchActivityController(IPunchActivityManager punchActivityManager)
        {
            _punchActivityManager = punchActivityManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetPunchActivities()
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<punchActivityListModel>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _punchActivityManager.GetPunchActivitiesAsync(ctx);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> SavePunchActivity([FromBody] punchActivityModel model)
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

            var response = await _punchActivityManager.SavePunchActivityAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> EditPunchActivity([FromBody] editPunchModel model)
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

            var response = await _punchActivityManager.EditPunchActivityAsync(ctx, model);

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