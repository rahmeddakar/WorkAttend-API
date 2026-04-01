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
    public class DakarIntegrationController : ControllerBase
    {
        private readonly IDakarIntegrationManager _dakarIntegrationManager;

        public DakarIntegrationController(IDakarIntegrationManager dakarIntegrationManager)
        {
            _dakarIntegrationManager = dakarIntegrationManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<dakarIntegrationMod>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _dakarIntegrationManager.GetConfigAsync(ctx);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConfig([FromBody] dakarIntegrationMod model)
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

            var response = await _dakarIntegrationManager.CreateConfigAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateConfig([FromBody] dakarIntegrationMod model)
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

            var response = await _dakarIntegrationManager.UpdateConfigAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("{companyConfigId:int}")]
        public async Task<IActionResult> DeleteConfig(int companyConfigId)
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

            var response = await _dakarIntegrationManager.DeleteConfigAsync(ctx, companyConfigId);

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