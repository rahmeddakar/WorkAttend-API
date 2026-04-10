using System;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.DakarIntegrationServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class DakarIntegrationManager : IDakarIntegrationManager
    {
        private readonly IDakarIntegrationService _dakarIntegrationService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public DakarIntegrationManager(
            IDakarIntegrationService dakarIntegrationService,
            IUserAccessContextManager userAccessContextManager)
        {
            _dakarIntegrationService = dakarIntegrationService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<dakarIntegrationMod>> GetConfigAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<dakarIntegrationMod>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "newcompany");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<dakarIntegrationMod>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var dakarConfig = await _dakarIntegrationService.GetDakarConfigAsync(accessContext.BaseCompanyId);

                dakarIntegrationMod model = new dakarIntegrationMod();

                if (dakarConfig != null && dakarConfig.companyConfigID > 0)
                {
                    model.companyConfigID = dakarConfig.companyConfigID;
                    model.DakarURL = dakarConfig.DakarURL;
                    model.CompanyCode = dakarConfig.companyCode;
                    model.SiteCode = dakarConfig.siteCode;
                }
                else
                {
                    model.companyConfigID = 0;
                }

                return new ApiResponse<dakarIntegrationMod>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = model
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "DakarIntegrationManager.GetConfigAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<dakarIntegrationMod>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateConfigAsync(CurrentUserContext ctx, dakarIntegrationMod model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "newcompany");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                if (model == null || string.IsNullOrWhiteSpace(model.DakarURL))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Something went wrong.",
                        Data = false
                    };
                }

                var dcc = await _dakarIntegrationService.CreateDakarCompanyConfigAsync(
                    accessContext.BaseCompanyId,
                    accessContext.UserId,
                    model.DakarURL,
                    model.CompanyCode,
                    model.SiteCode);

                if (dcc != null && dcc.companyConfigID > 0)
                {
                    bool isUpdated = await _dakarIntegrationService.UpdateDakarConnectedAsync(
                        accessContext.CompanyId,
                        accessContext.DatabaseName,
                        accessContext.UserId);

                    bool isUpdatedBase = await _dakarIntegrationService.UpdateDakarConnectedBaseAsync(
                        accessContext.BaseCompanyId,
                        accessContext.UserId);

                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = (isUpdated && isUpdatedBase) ? string.Empty : "Something went wrong.",
                        Data = isUpdated && isUpdatedBase
                    };
                }

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "DakarIntegrationManager.CreateConfigAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdateConfigAsync(CurrentUserContext ctx, dakarIntegrationMod model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "newcompany");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Update.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                if (model == null || model.companyConfigID <= 0 || string.IsNullOrWhiteSpace(model.DakarURL))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Something went wrong.",
                        Data = false
                    };
                }

                bool updated = await _dakarIntegrationService.UpdateDakarURLAsync(
                    model.companyConfigID,
                    model.DakarURL,
                    model.CompanyCode,
                    model.SiteCode,
                    accessContext.UserId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = string.Empty,
                    Data = updated
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "DakarIntegrationManager.UpdateConfigAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteConfigAsync(CurrentUserContext ctx, int companyConfigId)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "newcompany");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Delete.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                if (companyConfigId <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Something went wrong.",
                        Data = false
                    };
                }

                bool deleted = await _dakarIntegrationService.DeleteDakarURLAsync(companyConfigId, accessContext.UserId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = string.Empty,
                    Data = deleted
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "DakarIntegrationManager.DeleteConfigAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                };
            }
        }

        private async Task<UserAccessContext?> ResolveAccessContextAsync(CurrentUserContext ctx)
        {
            return await _userAccessContextManager.GetAsync(ctx);
        }
    }
}