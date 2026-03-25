using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.Model.Models.Auth;
using WorkAttend.SecurityToken;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAdminsManager _adminsManager;
        private readonly TokenGenerator _tokenGenerator;

        public AuthController(IAdminsManager adminsManager, TokenGenerator tokenGenerator)
        {
            _adminsManager = adminsManager;
            _tokenGenerator = tokenGenerator;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            try
            {
                AppLogger.Info(
                    message: "Login request received",
                    action: "Login",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"Email={request?.Email}, CompanyURL={request?.CompanyURL}");

                if (request == null)
                {
                    AppLogger.Warn(
                        message: "Login request body was missing",
                        action: "Login",
                        result: "Failed",
                        updatedBy: string.Empty,
                        description: "Request body is null");

                    return BadRequest(new LoginResponseModel
                    {
                        IsSuccess = false,
                        Message = "Request body is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.CompanyURL))
                {
                    AppLogger.Warn(
                        message: "Login failed because CompanyURL was empty",
                        action: "Login",
                        result: "Failed",
                        updatedBy: string.Empty,
                        description: $"Email={request.Email}, CompanyURL is required");

                    return BadRequest(new LoginResponseModel
                    {
                        IsSuccess = false,
                        Message = "CompanyURL is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    AppLogger.Warn(
                        message: "Login failed because email or password was missing",
                        action: "Login",
                        result: "Failed",
                        updatedBy: string.Empty,
                        description: $"Email={request.Email}, Email and password are required");

                    return BadRequest(new LoginResponseModel
                    {
                        IsSuccess = false,
                        Message = "Email and password are required."
                    });
                }

                var databaseName = await _adminsManager.GetDatabaseNameByCompanyUrlAsync(request.CompanyURL);

                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    AppLogger.Warn(
                        message: "Login failed because company URL was invalid",
                        action: "Login",
                        result: "Failed",
                        updatedBy: string.Empty,
                        description: $"Email={request.Email}, Invalid CompanyURL={request.CompanyURL}");

                    return Unauthorized(new LoginResponseModel
                    {
                        IsSuccess = false,
                        Message = "Invalid CompanyURL."
                    });
                }

                var admin = await _adminsManager.ValidateAdminAsync(databaseName, request.Email, request.Password);

                if (admin == null)
                {
                    AppLogger.Warn(
                        message: "Login failed because credentials were invalid",
                        action: "Login",
                        result: "Failed",
                        updatedBy: string.Empty,
                        description: $"Email={request.Email}, DatabaseName={databaseName}, Invalid email or password");

                    return Unauthorized(new LoginResponseModel
                    {
                        IsSuccess = false,
                        Message = "Invalid email or password."
                    });
                }

                var userAccessContext = await _adminsManager.GetUserAccessContextAsync(
                    admin.adminID.ToString(),
                    databaseName,
                    request.CompanyURL,
                    admin.email ?? request.Email);

                if (userAccessContext == null)
                {
                    AppLogger.Warn(
                        message: "Login failed because user access context could not be loaded",
                        action: "Login",
                        result: "Failed",
                        updatedBy: admin.adminID.ToString(),
                        description: $"Email={admin.email ?? request.Email}, DatabaseName={databaseName}, UserAccessContext was null");

                    return Unauthorized(new LoginResponseModel
                    {
                        IsSuccess = false,
                        Message = "Unable to load user access context."
                    });
                }

                var token = _tokenGenerator.GenerateToken(
                    userId: admin.adminID.ToString(),
                    userName: admin.name ?? request.Email,
                    email: admin.email ?? request.Email,
                    role: "Admin",
                    databaseName: databaseName,
                    companyUrl: request.CompanyURL
                );

                AppLogger.Info(
                    message: "Login completed successfully",
                    action: "Login",
                    result: "Success",
                    updatedBy: admin.adminID.ToString(),
                    description: $"Email={admin.email ?? request.Email}, DatabaseName={databaseName}, CompanyId={userAccessContext.CompanyId}, BaseCompanyId={userAccessContext.BaseCompanyId}");

                return Ok(new LoginResponseModel
                {
                    IsSuccess = true,
                    Message = "Login successful.",
                    Token = token,
                    Email = admin.email ?? request.Email,
                    UserName = admin.name ?? request.Email,
                    Role = "Admin",
                    DatabaseName = databaseName,
                    Policy = userAccessContext.Policy,
                    CompanyId = userAccessContext.CompanyId,
                    BaseCompanyId = userAccessContext.BaseCompanyId
                });
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Login failed with unexpected exception",
                    action: "Login",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"Email={request?.Email}, CompanyURL={request?.CompanyURL}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponseModel
                {
                    IsSuccess = false,
                    Message = "Something went wrong."
                });
            }
        }
    }
}