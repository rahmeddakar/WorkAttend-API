using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkAttend.API.Gateway.BLL.CommonCode.Helpers;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyManager _companyManager;

        public CompanyController(ICompanyManager companyManager)
        {
            _companyManager = companyManager;
        }

        [AllowAnonymous]
        [HttpGet("register-data")]
        public async Task<IActionResult> RegisterData()
        {
            try
            {
                var response = await _companyManager.GetRegisterDataAsync();

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Register data request failed with unexpected exception",
                    action: "Read",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                return StatusCode(500, new ApiResponse<RegisterCompanyPageData>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("company-url-exist")]
        public async Task<IActionResult> CompanyURLExist([FromBody] CompanyRegistrationCheckRequest model)
        {
            try
            {
                var response = await _companyManager.CompanyURLExistAsync(model);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Company registration check request failed with unexpected exception",
                    action: "Read",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={model?.companyURL}, PENumber={model?.peNumber}",
                    exception: ex);

                return StatusCode(500, new ApiResponse<CompanyRegistrationCheckResponse>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("register-company")]
        public async Task<IActionResult> RegisterCompany([FromBody] registerCompany registerModel)
        {
            try
            {
                var response = await _companyManager.RegisterCompanyAsync(registerModel);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Register company request failed with unexpected exception",
                    action: "Create",
                    result: "Failed",
                    updatedBy: registerModel?.adminEmail ?? string.Empty,
                    description: $"CompanyName={registerModel?.companyName}, CompanyURL={registerModel?.companyURL}",
                    exception: ex);

                return StatusCode(500, new ApiResponse<RegisterCompanyResult>
                {
                    Success = false,
                    Message = "Something went wrong. Try again creating a company.",
                    Data = null
                });
            }
        }

        [Authorize]
        [HttpPost("register-new-company")]
        public async Task<IActionResult> RegisterNewCompany([FromBody] registerNewCompany registerModel)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<RegisterNewCompanyResult>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _companyManager.RegisterNewCompanyAsync(ctx, registerModel);

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