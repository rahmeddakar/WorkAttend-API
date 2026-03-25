using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkAttend.API.Gateway.BLL.CommonCode.Helpers;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.Model.Models;
using WorkAttend.Model.Models.Admin;
using WorkAttend.SecurityToken;

namespace WorkAttend.API.Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly IAdminsManager _adminsManager;

        public AdminsController(IAdminsManager adminsManager)
        {
            _adminsManager = adminsManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
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

            var response = await _adminsManager.GetAdminsIndexDataAsync(ctx);
            return Ok(response);
        }

        [HttpGet("roles-overview")]
        public async Task<IActionResult> RolesOverview()
        {
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

            var response = await _adminsManager.GetRolesOverviewAsync(ctx);
            return Ok(response);
        }

        [HttpGet("getroles")]
        public async Task<IActionResult> GetRoles()
        {
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

            var response = await _adminsManager.GetRolesOnlyAsync(ctx);
            return Ok(response);
        }

        [HttpPost("createroles")]
        public async Task<IActionResult> CreateRole([FromBody] rolesDataModel rolesData)
        {
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

            var response = await _adminsManager.CreateRoleAsync(ctx, rolesData);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest model)
        {
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

            var response = await _adminsManager.CreateAdminAsync(ctx, model);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("delete-admin-{adminId:int}")]
        public async Task<IActionResult> DeleteAdmin(int adminId)
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

            var response = await _adminsManager.DeleteAdminAsync(ctx, adminId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("my-token-data")]
        public IActionResult GetMyTokenData()
        {
            return Ok(new
            {
                userId = User.FindFirst("userId")?.Value,
                companyId = User.FindFirst("companyId")?.Value,
                baseCompanyId = User.FindFirst("baseCompanyId")?.Value,
                databaseName = TokenGenerator.GetDatabaseName(User),
                userName = TokenGenerator.GetUserName(User),
                companyURL = TokenGenerator.GetCompanyURL(User),
                policy = User.FindFirst("policy")?.Value
            });
        }
    }
}