using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.services.AdminsServices;
using WorkAttend.API.Gateway.DAL.services.GeoFenceServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class GeoFenceManager : IGeoFenceManager
    {
        private readonly IGeoFenceService _geoFenceService;
        private readonly IUserAccessContextManager _userAccessContextManager;
        private readonly IAdminsService _adminsService;

        private const int GeoFencesFeatureId = (int)Constants.Features.Geofences;

        public GeoFenceManager(
            IGeoFenceService geoFenceService,
            IUserAccessContextManager userAccessContextManager,
            IAdminsService adminsService)
        {
            _geoFenceService = geoFenceService;
            _userAccessContextManager = userAccessContextManager;
            _adminsService = adminsService;
        }

        public async Task<ApiResponse<List<Location>>> GetGeoFencesAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "Geo-fence list load started",
                    action: "View",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Geo-fence list load blocked because access context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new ApiResponse<List<Location>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "geofence");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Geo-fence list load blocked by permission",
                        action: "View",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow view on geofence");

                    return new ApiResponse<List<Location>>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var data = await _geoFenceService.GetAllLocationsAsync(accessContext.CompanyId, accessContext.DatabaseName);

                AppLogger.Info(
                    message: "Geo-fence list load completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"Count={data.Count}");

                return new ApiResponse<List<Location>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Geo-fence list load failed",
                    action: "View",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await _geoFenceService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "GeoFenceManager.GetGeoFencesAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<List<Location>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateGeoFenceAsync(CurrentUserContext ctx, Location model)
        {
            try
            {
                AppLogger.Info(
                    message: "Create geo-fence started",
                    action: "Create",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"LocationName={model?.LocationName}, LocationCode={model?.LocationCode}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Create geo-fence blocked because access context was not found",
                        action: "Create",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"LocationName={model?.LocationName}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var companySub = await _adminsService.GetActivePackageData(accessContext.BaseCompanyId, GeoFencesFeatureId);
                var companyFeatureValues = await _adminsService.GetCompanySubscriptionFeatures(accessContext.BaseCompanyId, GeoFencesFeatureId);
                var currentLocationCount = await _geoFenceService.GetDatabaseLocationCountAsync(accessContext.DatabaseName);

                if (companySub == null)
                {
                    AppLogger.Warn(
                        message: "Create geo-fence blocked because subscription information was not found",
                        action: "Create",
                        result: "SubscriptionMissing",
                        updatedBy: accessContext.UserId,
                        description: $"BaseCompanyId={accessContext.BaseCompanyId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Subscription information not found.",
                        Data = false
                    };
                }

                var maxLocationCount =
                    companyFeatureValues != null && companyFeatureValues.Count > 0
                        ? companyFeatureValues.First().FeatureValue
                        : companySub.FeatureValue;

                if (!(currentLocationCount < maxLocationCount || companySub.FeatureValue == -1))
                {
                    AppLogger.Warn(
                        message: "Create geo-fence blocked by subscription limit",
                        action: "Create",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"CurrentLocationCount={currentLocationCount}, MaxLocationCount={maxLocationCount}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Subscription limit reached.",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "geofence");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Create geo-fence blocked by permission",
                        action: "Create",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"LocationName={model?.LocationName}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                bool isSaved = await _geoFenceService.CreateGeoFenceAsync(
                    accessContext.UserId,
                    accessContext.CompanyId,
                    model,
                    accessContext.DatabaseName);

                if (!isSaved)
                {
                    AppLogger.Warn(
                        message: "Create geo-fence failed because database operation did not complete",
                        action: "Create",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"LocationName={model?.LocationName}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to save geo-fence.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Create geo-fence completed successfully",
                    action: "Create",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"LocationName={model?.LocationName}, CompanyId={accessContext.CompanyId}");

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
                    message: "Create geo-fence failed",
                    action: "Create",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"LocationName={model?.LocationName}",
                    exception: ex);

                await _geoFenceService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "GeoFenceManager.CreateGeoFenceAsync",
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

        public async Task<ApiResponse<bool>> DeleteGeoFenceAsync(CurrentUserContext ctx, int id)
        {
            try
            {
                AppLogger.Info(
                    message: "Delete geo-fence started",
                    action: "Delete",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"LocationId={id}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Delete geo-fence blocked because access context was not found",
                        action: "Delete",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"LocationId={id}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "geofence");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Delete.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Delete geo-fence blocked by permission",
                        action: "Delete",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"LocationId={id}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                bool isAssigned = await _geoFenceService.IsLocationAssignedToEmployeeAsync(id, accessContext.DatabaseName);
                if (isAssigned)
                {
                    AppLogger.Warn(
                        message: "Delete geo-fence blocked because it is assigned to employees",
                        action: "Delete",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"LocationId={id}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to delete Geo-Fence. It is assigned to employee(s).",
                        Data = false
                    };
                }

                bool isDeleted = await _geoFenceService.SoftDeleteLocationAsync(id, accessContext.UserId, accessContext.DatabaseName);

                if (!isDeleted)
                {
                    AppLogger.Warn(
                        message: "Delete geo-fence failed because database operation did not complete",
                        action: "Delete",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"LocationId={id}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to delete Geo-Fence.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Delete geo-fence completed successfully",
                    action: "Delete",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"LocationId={id}");

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
                    message: "Delete geo-fence failed",
                    action: "Delete",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"LocationId={id}",
                    exception: ex);

                await _geoFenceService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "GeoFenceManager.DeleteGeoFenceAsync",
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

        public async Task<ApiResponse<bool>> EditGeoFenceAsync(CurrentUserContext ctx, Location model)
        {
            try
            {
                AppLogger.Info(
                    message: "Edit geo-fence started",
                    action: "Update",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"LocationId={model?.Id}, LocationName={model?.LocationName}, LocationCode={model?.LocationCode}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Edit geo-fence blocked because access context was not found",
                        action: "Update",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"LocationId={model?.Id}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "geofence");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Update.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Edit geo-fence blocked by permission",
                        action: "Update",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"LocationId={model?.Id}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                if (model == null || model.Id <= 0)
                {
                    AppLogger.Warn(
                        message: "Edit geo-fence request validation failed",
                        action: "Update",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: $"LocationId={model?.Id}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                var savedLocation = await _geoFenceService.GetLocationAsync(model.Id, accessContext.DatabaseName);
                if (savedLocation == null || savedLocation.locationID <= 0)
                {
                    AppLogger.Warn(
                        message: "Edit geo-fence failed because existing location was not found",
                        action: "Update",
                        result: "NotFound",
                        updatedBy: accessContext.UserId,
                        description: $"LocationId={model.Id}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to update Geo-Fence.",
                        Data = false
                    };
                }

                savedLocation.locationCode = model.LocationCode;
                savedLocation.locationName = model.LocationName;

                bool isSaved = await _geoFenceService.UpdateLocationAsync(accessContext.UserId, savedLocation, accessContext.DatabaseName);

                if (!isSaved)
                {
                    AppLogger.Warn(
                        message: "Edit geo-fence failed because database operation did not complete",
                        action: "Update",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"LocationId={model.Id}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to update Geo-Fence.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Edit geo-fence completed successfully",
                    action: "Update",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"LocationId={model.Id}, LocationName={model.LocationName}, LocationCode={model.LocationCode}");

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
                    message: "Edit geo-fence failed",
                    action: "Update",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"LocationId={model?.Id}",
                    exception: ex);

                await _geoFenceService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "GeoFenceManager.EditGeoFenceAsync",
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