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
    public class PunchHistoryMapController : ControllerBase
    {
        private readonly IPunchHistoryMapManager _punchHistoryMapManager;

        public PunchHistoryMapController(IPunchHistoryMapManager punchHistoryMapManager)
        {
            _punchHistoryMapManager = punchHistoryMapManager;
        }

        [HttpPost("page-data")]
        public async Task<IActionResult> GetPageData([FromBody] punchHistoryMap model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<punchHistoryMap>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _punchHistoryMapManager.GetPageDataAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("filter-data")]
        public async Task<IActionResult> GetFilterData([FromBody] punchHistoryMap model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<punchHistoryMap>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _punchHistoryMapManager.GetFilterDataAsync(ctx, model);

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