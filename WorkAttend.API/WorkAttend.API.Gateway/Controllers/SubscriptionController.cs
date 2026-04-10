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
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionManager _subscriptionManager;

        public SubscriptionController(ISubscriptionManager subscriptionManager)
        {
            _subscriptionManager = subscriptionManager;
        }

        [HttpGet("page-data")]
        public async Task<IActionResult> GetPageData()
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<subscription>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _subscriptionManager.GetPageDataAsync(ctx);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("toggle-feature")]
        public async Task<IActionResult> ToggleSubscriptionFeature([FromBody] ConfigurableFeature model)
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

            var response = await _subscriptionManager.ToggleSubscriptionFeatureAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("subscribe-plan")]
        public async Task<IActionResult> SubscribePlan([FromBody] subscribePlan model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<SiteTokenModel>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _subscriptionManager.SubscribePlanAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("payment/status")]
        public async Task<IActionResult> ConfirmPaymentStatus([FromQuery] string userId, [FromQuery] string token)
        {
            var response = await _subscriptionManager.ConfirmPaymentStatusAsync(userId, token);
            return Content(response ?? string.Empty, "text/plain");
        }

        [AllowAnonymous]
        [HttpGet("payment/response")]
        public async Task<IActionResult> ConfirmPaymentRedirect([FromQuery] string token, [FromQuery] string userId)
        {
            var url = await _subscriptionManager.GetPaymentRedirectUrlAsync(userId, token);
            return Redirect(url);
        }

        [HttpPost("address")]
        public async Task<IActionResult> AddAddress([FromBody] billingAddress model)
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

            var response = await _subscriptionManager.AddAddressAsync(ctx, model);

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