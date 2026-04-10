using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.AdminsServices;
using WorkAttend.Model.Models;
using WorkAttend.Model.Models.Admin;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class AdminsManager : IAdminsManager
    {
        private readonly IAdminsService _adminsService;
        private readonly IConfiguration _configuration;
        private readonly IUserAccessContextManager _userAccessContextManager;

        private const int AdminsFeatureId = (int)Constants.Features.Admins;

        public AdminsManager(
            IAdminsService adminsService,
            IConfiguration configuration,
            IUserAccessContextManager userAccessContextManager)
        {
            _adminsService = adminsService;
            _configuration = configuration;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<string?> GetDatabaseNameByCompanyUrlAsync(string companyUrl)
        {
            try
            {
                AppLogger.Info(
                    message: "Resolving database name by company URL",
                    action: "Login",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={companyUrl}");

                var databaseName = await _adminsService.GetDatabaseNameByCompanyUrlAsync(companyUrl);

                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    AppLogger.Warn(
                        message: "Database name not found for company URL",
                        action: "Login",
                        result: "NotFound",
                        updatedBy: string.Empty,
                        description: $"CompanyURL={companyUrl}");

                    return null;
                }

                AppLogger.Info(
                    message: "Database name resolved successfully",
                    action: "Login",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={companyUrl}, DatabaseName={databaseName}");

                return databaseName;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to resolve database name by company URL",
                    action: "Login",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={companyUrl}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminsManager.GetDatabaseNameByCompanyUrlAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return null;
            }
        }

        public async Task<workattendadmin?> ValidateAdminAsync(string databaseName, string email, string password)
        {
            try
            {
                AppLogger.Info(
                    message: "Admin validation started",
                    action: "Login",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Email={email}");

                string encryptedPassword = EncryptionHelper.Encrypt(password);
                var admin = await _adminsService.ValidateAdminAsync(databaseName, email, encryptedPassword);

                if (admin == null)
                {
                    AppLogger.Warn(
                        message: "Admin validation failed",
                        action: "Login",
                        result: "Failed",
                        updatedBy: string.Empty,
                        description: $"DatabaseName={databaseName}, Email={email}");

                    return null;
                }

                AppLogger.Info(
                    message: "Admin validated successfully",
                    action: "Login",
                    result: "Success",
                    updatedBy: admin.adminID.ToString(),
                    description: $"DatabaseName={databaseName}, Email={admin.email}");

                return admin;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Admin validation failed with exception",
                    action: "Login",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Email={email}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminsManager.ValidateAdminAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return null;
            }
        }

        public async Task<UserAccessContext?> GetUserAccessContextAsync(string userId, string databaseName, string companyURL, string email)
        {
            try
            {
                AppLogger.Info(
                    message: "Loading user access context",
                    action: "Authorization",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyURL={companyURL}, Email={email}");

                var currentUserContext = new CurrentUserContext
                {
                    UserId = userId,
                    DatabaseName = databaseName,
                    CompanyURL = companyURL,
                    Email = email
                };

                var accessContext = await _userAccessContextManager.GetAsync(currentUserContext, forceRefresh: true);

                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "User access context was not found",
                        action: "Authorization",
                        result: "NotFound",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, CompanyURL={companyURL}, Email={email}");

                    return null;
                }

                AppLogger.Info(
                    message: "User access context loaded successfully",
                    action: "Authorization",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={accessContext.CompanyId}, BaseCompanyId={accessContext.BaseCompanyId}, RoleId={accessContext.RoleId}");

                return accessContext;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load user access context",
                    action: "Authorization",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyURL={companyURL}, Email={email}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminsManager.GetUserAccessContextAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return null;
            }
        }

        private async Task<UserAccessContext?> ResolveAccessContextAsync(CurrentUserContext ctx)
        {
            return await _userAccessContextManager.GetAsync(ctx);
        }

        public async Task<AdminsIndexResponse> GetAdminsIndexDataAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "Loading admins index data",
                    action: "Read",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Admins index data load blocked because access context was not found",
                        action: "Read",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new AdminsIndexResponse();
                }

                var roles = await _adminsService.GetRoles(accessContext.CompanyId, accessContext.DatabaseName);
                var admins = await _adminsService.GetAllAdmins(accessContext.CompanyId, accessContext.DatabaseName);

                AppLogger.Info(
                    message: "Admins index data loaded successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"CompanyId={accessContext.CompanyId}, RolesCount={roles?.Count ?? 0}, AdminsCount={admins?.Count ?? 0}");

                return new AdminsIndexResponse
                {
                    roles = roles,
                    adminList = admins
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load admins index data",
                    action: "Read",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminsManager.GetAdminsIndexDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new AdminsIndexResponse();
            }
        }

        public async Task<RolesOverviewResponse> GetRolesOverviewAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "Loading roles overview",
                    action: "Read",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Roles overview load blocked because access context was not found",
                        action: "Read",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new RolesOverviewResponse();
                }

                var roles = await _adminsService.GetRoles(accessContext.CompanyId, accessContext.DatabaseName);
                var permissions = await _adminsService.GetPermissions(accessContext.DatabaseName);

                var roleData = new List<RoleOverviewDto>();

                foreach (var item in roles)
                {
                    var count = await _adminsService.GetRolesCount(accessContext.CompanyId, item.roleID, accessContext.DatabaseName);

                    roleData.Add(new RoleOverviewDto
                    {
                        roleID = item.roleID,
                        policy = item.policy,
                        roleName = item.name,
                        roleDescription = item.description,
                        numberOfAdmins = count
                    });
                }

                AppLogger.Info(
                    message: "Roles overview loaded successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"CompanyId={accessContext.CompanyId}, RolesCount={roleData.Count}, PermissionsCount={permissions?.Count ?? 0}");

                return new RolesOverviewResponse
                {
                    roleDataList = roleData,
                    permissions = permissions
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load roles overview",
                    action: "Read",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminsManager.GetRolesOverviewAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new RolesOverviewResponse();
            }
        }

        public async Task<List<Roles>> GetRolesOnlyAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "Loading roles list",
                    action: "Read",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Roles list load blocked because access context was not found",
                        action: "Read",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new List<Roles>();
                }

                var roles = await _adminsService.GetRoles(accessContext.CompanyId, accessContext.DatabaseName);

                AppLogger.Info(
                    message: "Roles list loaded successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"CompanyId={accessContext.CompanyId}, RolesCount={roles?.Count ?? 0}");

                return roles;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load roles list",
                    action: "Read",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminsManager.GetRolesOnlyAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new List<Roles>();
            }
        }

        public async Task<ApiResponse<object>> CreateRoleAsync(CurrentUserContext ctx, rolesDataModel model)
        {
            try
            {
                AppLogger.Info(
                    message: "Create role started",
                    action: "Create",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"RoleName={model?.roleName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Create role blocked because access context was not found",
                        action: "Create",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"RoleName={model?.roleName}");

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(model.roleName) ||
                    string.IsNullOrWhiteSpace(model.roleDescription) ||
                    string.IsNullOrWhiteSpace(model.policy))
                {
                    AppLogger.Warn(
                        message: "Create role request validation failed",
                        action: "Create",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: $"RoleName={model?.roleName}");

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating a role.",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "admins");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Create role blocked by permission",
                        action: "Create",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow create on admins");

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var role = await _adminsService.AddRole(
                    accessContext.CompanyId,
                    model.policy,
                    accessContext.DatabaseName,
                    model.roleName,
                    model.roleDescription,
                    accessContext.UserId);

                if (role != null)
                {
                    AppLogger.Info(
                        message: "Create role completed successfully",
                        action: "Create",
                        result: "Success",
                        updatedBy: accessContext.UserId,
                        description: $"RoleId={role.roleID}, RoleName={role.name}, CompanyId={accessContext.CompanyId}");

                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Created Successfully.",
                        Data = null
                    };
                }

                AppLogger.Warn(
                    message: "Create role failed because role was not created",
                    action: "Create",
                    result: "Failed",
                    updatedBy: accessContext.UserId,
                    description: $"RoleName={model?.roleName}, CompanyId={accessContext.CompanyId}");

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Information not provided.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Create role failed",
                    action: "Create",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"RoleName={model?.roleName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminsManager.CreateRoleAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<object>> CreateAdminAsync(CurrentUserContext ctx, CreateAdminRequest model)
        {
            try
            {
                AppLogger.Info(
                    message: "Create admin started",
                    action: "Create",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"AdminEmail={model?.adminEmail}, RoleId={model?.roleID}");

                var accessContext = await ResolveAccessContextAsync(ctx);

                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Create admin blocked because access context was not found",
                        action: "Create",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"AdminEmail={model?.adminEmail}");

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(model.adminEmail) || model.roleID <= 0)
                {
                    AppLogger.Warn(
                        message: "Create admin request validation failed",
                        action: "Create",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: $"AdminEmail={model?.adminEmail}, RoleId={model?.roleID}");

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating an admin.",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "admins");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Create admin blocked by permission",
                        action: "Create",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow create on admins");

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var companySub = await _adminsService.GetActivePackageData(accessContext.BaseCompanyId, AdminsFeatureId);
                var companyFeatureValues = await _adminsService.GetCompanySubscriptionFeatures(accessContext.BaseCompanyId, AdminsFeatureId);
                var currentAdminCount = await _adminsService.GetDatabaseAdminCount(accessContext.DatabaseName);

                if (companySub == null)
                {
                    AppLogger.Warn(
                        message: "Create admin blocked because subscription information was not found",
                        action: "Create",
                        result: "SubscriptionMissing",
                        updatedBy: accessContext.UserId,
                        description: $"BaseCompanyId={accessContext.BaseCompanyId}");

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Subscription information not found.",
                        Data = null
                    };
                }

                var maxAdminsCount =
                    companyFeatureValues != null && companyFeatureValues.Count > 0
                        ? companyFeatureValues.First().FeatureValue
                        : companySub.FeatureValue;

                if (!(currentAdminCount < maxAdminsCount || companySub.FeatureValue == -1))
                {
                    AppLogger.Warn(
                        message: "Create admin blocked by subscription limit",
                        action: "Create",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"CurrentAdminCount={currentAdminCount}, MaxAdminsCount={maxAdminsCount}");

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Subscription not allowed.",
                        Data = null
                    };
                }

                var generatedPassword = StringHelper.GetRandomAlphanumericString(8);

                var existingAdmin = await _adminsService.GetAdminFromEmail(model.adminEmail, accessContext.DatabaseName);

                if (existingAdmin != null && existingAdmin.adminID > 0)
                {
                    var companyAdmin = await _adminsService.CheckAdminCompanyExist(existingAdmin.adminID, accessContext.CompanyId, accessContext.DatabaseName);

                    if (companyAdmin != null && companyAdmin.companyAdminId > 0)
                    {
                        AppLogger.Warn(
                            message: "Create admin blocked because record already exists",
                            action: "Create",
                            result: "Failed",
                            updatedBy: accessContext.UserId,
                            description: $"AdminEmail={model.adminEmail}, ExistingAdminId={existingAdmin.adminID}, CompanyId={accessContext.CompanyId}");

                        return new ApiResponse<object>
                        {
                            Success = false,
                            Message = "record already exist.",
                            Data = null
                        };
                    }

                    await _adminsService.AddCompanyAdmin(existingAdmin.adminID, accessContext.CompanyId, accessContext.DatabaseName);
                    await _adminsService.AddAdminRole(existingAdmin.adminID, model.roleID, accessContext.UserId, accessContext.DatabaseName, accessContext.CompanyId);

                    await EmailHelper.SendRegisterEmail(
                        _configuration,
                        model.adminEmail,
                        model.adminName,
                        accessContext.CompanyURL,
                        generatedPassword);

                    AppLogger.Info(
                        message: "Existing admin linked and assigned successfully",
                        action: "Create",
                        result: "Success",
                        updatedBy: accessContext.UserId,
                        description: $"AdminEmail={model.adminEmail}, ExistingAdminId={existingAdmin.adminID}, CompanyId={accessContext.CompanyId}, RoleId={model.roleID}");

                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Created Successfully.",
                        Data = null
                    };
                }

                var newAdmin = await _adminsService.AddAdmin(
                    model.adminEmail,
                    generatedPassword,
                    accessContext.UserId,
                    accessContext.DatabaseName,
                    model.adminName);

                if (newAdmin == null || newAdmin.adminID <= 0)
                {
                    AppLogger.Warn(
                        message: "Create admin failed because admin record was not created",
                        action: "Create",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"AdminEmail={model.adminEmail}");

                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Something went wrong.",
                        Data = null
                    };
                }

                await _adminsService.AddCompanyAdmin(newAdmin.adminID, accessContext.CompanyId, accessContext.DatabaseName);
                await _adminsService.AddAdminRole(newAdmin.adminID, model.roleID, accessContext.UserId, accessContext.DatabaseName, accessContext.CompanyId);

                await EmailHelper.SendRegisterEmail(
                    _configuration,
                    model.adminEmail,
                    model.adminName,
                    accessContext.CompanyURL,
                    generatedPassword);

                AppLogger.Info(
                    message: "Create admin completed successfully",
                    action: "Create",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"AdminEmail={model.adminEmail}, NewAdminId={newAdmin.adminID}, CompanyId={accessContext.CompanyId}, RoleId={model.roleID}");

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Created Successfully.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Create admin failed",
                    action: "Create",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"AdminEmail={model?.adminEmail}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminsManager.CreateAdminAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAdminAsync(CurrentUserContext ctx, int adminId)
        {
            try
            {
                AppLogger.Info(
                    message: "Delete admin started",
                    action: "Delete",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"AdminId={adminId}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Delete admin blocked because access context was not found",
                        action: "Delete",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"AdminId={adminId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "admins");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Delete.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Delete admin blocked by permission",
                        action: "Delete",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"AdminId={adminId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                var isDeleted = await _adminsService.DeleteAdmin(adminId, accessContext.UserId, accessContext.CompanyId, accessContext.DatabaseName);

                if (!isDeleted)
                {
                    AppLogger.Warn(
                        message: "Delete admin was not completed",
                        action: "Delete",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"AdminId={adminId}, CompanyId={accessContext.CompanyId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to delete admin.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Delete admin completed successfully",
                    action: "Delete",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"AdminId={adminId}, CompanyId={accessContext.CompanyId}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Delete admin failed",
                    action: "Delete",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"AdminId={adminId}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "AdminsManager.DeleteAdminAsync",
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
    }
}