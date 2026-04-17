using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.AuthServices;
using WorkAttend.Model.Models;
using WorkAttend.Model.Models.Auth;
using WorkAttend.Shared.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WorkAttend.SecurityToken;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class AuthManager : IAuthManager
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly TokenGenerator _tokenGenerator;

        public AuthManager(
            IAuthService authService,
            IConfiguration configuration,
            TokenGenerator tokenGenerator)
        {
            _authService = authService;
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<ApiResponse<bool>> CompanyExistsAsync(CompanyExistsRequest request)
        {
            try
            {
                AppLogger.Info(
                    message: "Company existence check started",
                    action: "Read",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={request?.companyURL}");

                if (request == null || string.IsNullOrWhiteSpace(request.companyURL))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Please provide company name to continue.",
                        Data = false
                    };
                }

                var companyData = await _authService.GetCompanyConfigurationAsync(request.companyURL.Trim());
                bool exists = companyData != null && companyData.companyID > 0;

                AppLogger.Info(
                    message: "Company existence check completed successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={request.companyURL}, Exists={exists}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = string.Empty,
                    Data = exists
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Company existence check failed",
                    action: "Read",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={request?.companyURL}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AuthManager.CompanyExistsAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong. Try again.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                AppLogger.Info(
                    message: "Forgot password started",
                    action: "ForgotPassword",
                    result: "Started",
                    updatedBy: request?.email ?? string.Empty,
                    description: $"CompanyURL={request?.companyURL}");

                if (request == null ||
                    string.IsNullOrWhiteSpace(request.companyURL) ||
                    string.IsNullOrWhiteSpace(request.email))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Please fill all the fields correctly.",
                        Data = false
                    };
                }

                var companyData = await _authService.GetCompanyConfigurationAsync(request.companyURL.Trim());
                if (companyData == null || companyData.companyID <= 0 || string.IsNullOrWhiteSpace(companyData.databaseName))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Access denied.",
                        Data = false
                    };
                }

                bool isSubscriptionValid = await _authService.GetActivePackageAsync(companyData.companyID);
                if (!isSubscriptionValid)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Access denied.",
                        Data = false
                    };
                }

                var admin = await _authService.GetAdminFromEmailAsync(request.email.Trim(), companyData.databaseName);
                if (admin == null || admin.adminID <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Couldn't find your WorkAttend Account.",
                        Data = false
                    };
                }

                bool isRedundantRequest = await _authService.IsRedundantRequestAsync(admin.adminID, companyData.databaseName);
                if (isRedundantRequest)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Too many reset requests. Please wait for 30 min to try again resetting the password.",
                        Data = false
                    };
                }

                string token = Guid.NewGuid().ToString();
                string encryptedAdminId = EncryptionHelper.Encrypt(admin.adminID.ToString());
                string resetUrl = $"{BuildCompanySubdomainUrl(request.companyURL.Trim(), "reset-password")}?token={Uri.EscapeDataString(token)}&identity={Uri.EscapeDataString(encryptedAdminId)}";

                var storedToken = await _authService.StorePasswordResetTokenAsync(
                    admin.email ?? request.email.Trim(),
                    admin.adminID,
                    token,
                    companyData.databaseName);

                if (storedToken == null || storedToken.adminresettokenID <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again.",
                        Data = false
                    };
                }

                bool isLinkSent = await EmailHelper.SendPasswordResetEmail(
                    _configuration,
                    admin.email ?? request.email.Trim(),
                    admin.name ?? request.email.Trim(),
                    resetUrl);

                if (!isLinkSent)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to send reset link.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Forgot password completed successfully",
                    action: "ForgotPassword",
                    result: "Success",
                    updatedBy: request.email,
                    description: $"CompanyURL={request.companyURL}, AdminId={admin.adminID}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Password reset link sent successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Forgot password failed",
                    action: "ForgotPassword",
                    result: "Failed",
                    updatedBy: request?.email ?? string.Empty,
                    description: $"CompanyURL={request?.companyURL}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AuthManager.ForgotPasswordAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong. Try again.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> ValidateResetTokenAsync(ValidateResetTokenRequest request)
        {
            try
            {
                AppLogger.Info(
                    message: "Validate reset token started",
                    action: "ValidateResetToken",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={request?.companyURL}");

                if (request == null ||
                    string.IsNullOrWhiteSpace(request.companyURL) ||
                    string.IsNullOrWhiteSpace(request.token) ||
                    string.IsNullOrWhiteSpace(request.identity))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid reset link.",
                        Data = false
                    };
                }

                var companyData = await _authService.GetCompanyConfigurationAsync(request.companyURL.Trim());
                if (companyData == null || companyData.companyID <= 0 || string.IsNullOrWhiteSpace(companyData.databaseName))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid reset link.",
                        Data = false
                    };
                }

                if (!TryDecryptAdminId(request.identity, out int adminId))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid reset link.",
                        Data = false
                    };
                }

                var tokenData = await _authService.CheckTokenExistAsync(adminId, request.token.Trim(), companyData.databaseName);
                bool isValid = tokenData != null && tokenData.adminresettokenID > 0;

                AppLogger.Info(
                    message: "Validate reset token completed",
                    action: "ValidateResetToken",
                    result: isValid ? "Success" : "Failed",
                    updatedBy: adminId.ToString(),
                    description: $"CompanyURL={request.companyURL}");

                return new ApiResponse<bool>
                {
                    Success = isValid,
                    Message = isValid ? "Reset token is valid." : "Reset link is invalid or expired.",
                    Data = isValid
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Validate reset token failed",
                    action: "ValidateResetToken",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={request?.companyURL}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AuthManager.ValidateResetTokenAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong. Try again.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdatePasswordAsync(UpdatePasswordRequest request)
        {
            try
            {
                AppLogger.Info(
                    message: "Update password started",
                    action: "UpdatePassword",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={request?.companyURL}");

                if (request == null ||
                    string.IsNullOrWhiteSpace(request.companyURL) ||
                    string.IsNullOrWhiteSpace(request.password) ||
                    string.IsNullOrWhiteSpace(request.token) ||
                    string.IsNullOrWhiteSpace(request.identity))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Please fill all the fields correctly.",
                        Data = false
                    };
                }

                var companyData = await _authService.GetCompanyConfigurationAsync(request.companyURL.Trim());
                if (companyData == null || companyData.companyID <= 0 || string.IsNullOrWhiteSpace(companyData.databaseName))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Access denied.",
                        Data = false
                    };
                }

                bool isSubscriptionValid = await _authService.GetActivePackageAsync(companyData.companyID);
                if (!isSubscriptionValid)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Access denied.",
                        Data = false
                    };
                }

                if (!TryDecryptAdminId(request.identity, out int adminId))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid reset link.",
                        Data = false
                    };
                }

                var tokenData = await _authService.CheckTokenExistAsync(adminId, request.token.Trim(), companyData.databaseName);
                if (tokenData == null || tokenData.adminresettokenID <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Reset link is invalid or expired.",
                        Data = false
                    };
                }

                bool isUpdated = await _authService.UpdateAdminPasswordAsync(adminId, request.password.Trim(), companyData.databaseName);
                if (!isUpdated)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again.",
                        Data = false
                    };
                }

                var tokenIds = await _authService.GetAdminTokensAsync(companyData.databaseName, adminId);
                await _authService.ExpireAllTokensAsync(companyData.databaseName, tokenIds, adminId);

                AppLogger.Info(
                    message: "Update password completed successfully",
                    action: "UpdatePassword",
                    result: "Success",
                    updatedBy: adminId.ToString(),
                    description: $"CompanyURL={request.companyURL}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Password updated successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Update password failed",
                    action: "UpdatePassword",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={request?.companyURL}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AuthManager.UpdatePasswordAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong. Try again.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<AuthTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                AppLogger.Info(
                    message: "Refresh token flow started",
                    action: "RefreshToken",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={request?.companyURL}");

                if (request == null ||
                    string.IsNullOrWhiteSpace(request.companyURL) ||
                    string.IsNullOrWhiteSpace(request.accessToken) ||
                    string.IsNullOrWhiteSpace(request.refreshToken))
                {
                    return new ApiResponse<AuthTokenResponse>
                    {
                        Success = false,
                        Message = "Invalid refresh request.",
                        Data = null
                    };
                }

                var companyData = await _authService.GetCompanyConfigurationAsync(request.companyURL.Trim());
                if (companyData == null || companyData.companyID <= 0 || string.IsNullOrWhiteSpace(companyData.databaseName))
                {
                    return new ApiResponse<AuthTokenResponse>
                    {
                        Success = false,
                        Message = "Invalid refresh request.",
                        Data = null
                    };
                }

                var savedRefreshToken = await _authService.GetValidRefreshTokenAsync(
                    request.refreshToken.Trim(),
                    companyData.databaseName);

                if (savedRefreshToken == null || savedRefreshToken.adminrefreshtokenID <= 0)
                {
                    return new ApiResponse<AuthTokenResponse>
                    {
                        Success = false,
                        Message = "Session expired. Please login again.",
                        Data = null
                    };
                }

                ClaimsPrincipal principal;
                try
                {
                    principal = _tokenGenerator.GetPrincipalFromExpiredToken(request.accessToken.Trim());
                }
                catch
                {
                    return new ApiResponse<AuthTokenResponse>
                    {
                        Success = false,
                        Message = "Invalid access token.",
                        Data = null
                    };
                }

                string userIdFromToken =
                    principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
                    string.Empty;

                if (!int.TryParse(userIdFromToken, out int adminId) || adminId <= 0 || adminId != savedRefreshToken.adminID)
                {
                    return new ApiResponse<AuthTokenResponse>
                    {
                        Success = false,
                        Message = "Invalid refresh request.",
                        Data = null
                    };
                }

                var admin = await _authService.GetAdminByIdAsync(adminId, companyData.databaseName);
                if (admin == null || admin.adminID <= 0 || admin.isDeleted)
                {
                    return new ApiResponse<AuthTokenResponse>
                    {
                        Success = false,
                        Message = "Session expired. Please login again.",
                        Data = null
                    };
                }

                string role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "Admin";
                string userName = TokenGenerator.GetUserName(principal) ?? admin.name ?? string.Empty;
                string email = principal.FindFirst(ClaimTypes.Email)?.Value ?? admin.email ?? string.Empty;
                string databaseName = TokenGenerator.GetDatabaseName(principal) ?? companyData.databaseName;
                string companyUrl = TokenGenerator.GetCompanyURL(principal) ?? request.companyURL.Trim();

                string newAccessToken = _tokenGenerator.GenerateToken(
                    admin.adminID.ToString(),
                    userName,
                    email,
                    role,
                    databaseName,
                    companyUrl);

                string newRefreshToken = _tokenGenerator.GenerateRefreshToken();

                await _authService.ExpireRefreshTokenAsync(
                    savedRefreshToken.adminrefreshtokenID,
                    email,
                    companyData.databaseName);

                var newSavedRefreshToken = await _authService.StoreRefreshTokenAsync(
                    admin.adminID,
                    newRefreshToken,
                    companyData.databaseName,
                    email);

                if (newSavedRefreshToken == null || newSavedRefreshToken.adminrefreshtokenID <= 0)
                {
                    return new ApiResponse<AuthTokenResponse>
                    {
                        Success = false,
                        Message = "Unable to refresh session.",
                        Data = null
                    };
                }

                return new ApiResponse<AuthTokenResponse>
                {
                    Success = true,
                    Message = "Token refreshed successfully.",
                    Data = new AuthTokenResponse
                    {
                        accessToken = newAccessToken,
                        refreshToken = newRefreshToken,
                        expiresInMinutes = _tokenGenerator.GetExpireMinutes(),
                        userId = admin.adminID.ToString(),
                        userName = userName,
                        email = email,
                        role = role,
                        companyURL = companyUrl
                    }
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Refresh token flow failed",
                    action: "RefreshToken",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={request?.companyURL}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AuthManager.RefreshTokenAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<AuthTokenResponse>
                {
                    Success = false,
                    Message = "Something went wrong. Try again.",
                    Data = null
                };
            }
        }

        private bool TryDecryptAdminId(string encryptedIdentity, out int adminId)
        {
            adminId = 0;

            try
            {
                string decrypted = EncryptionHelper.Decrypt(encryptedIdentity);
                return int.TryParse(decrypted, out adminId) && adminId > 0;
            }
            catch
            {
                return false;
            }
        }

        private string BuildCompanySubdomainUrl(string companyURL, string path)
        {
            string baseUrl =
                _configuration["AppSettings:companyBaseURL"] ??
                _configuration["companyBaseURL"] ??
                string.Empty;

            baseUrl = baseUrl.Trim().TrimEnd('/');
            path = path.TrimStart('/');

            if (baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return $"{baseUrl}/{path}";
            }

            return $"https://{companyURL}.{baseUrl}/{path}";
        }
    }
}