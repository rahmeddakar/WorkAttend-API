using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.services.ActivityServices;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class ActivityManager : IActivityManager
    {
        private readonly IActivityService _activityService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public ActivityManager(
            IActivityService activityService,
            IUserAccessContextManager userAccessContextManager)
        {
            _activityService = activityService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<List<Activity>>> GetActivitiesAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "Get activities started",
                    action: "View",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Get activities blocked because access context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new ApiResponse<List<Activity>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "activity");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Get activities blocked by permission",
                        action: "View",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow view on activity");

                    return new ApiResponse<List<Activity>>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var activities = await _activityService.GetAllActivitiesAsync(accessContext.DatabaseName);

                AppLogger.Info(
                    message: "Get activities completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"ActivitiesCount={activities.Count}");

                return new ApiResponse<List<Activity>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = activities
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Get activities failed",
                    action: "View",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "ActivityManager.GetActivitiesAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<List<Activity>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> SaveActivityAsync(CurrentUserContext ctx, projectClientList model)
        {
            try
            {
                AppLogger.Info(
                    message: "Save activity started",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"ActivityId={model?.activityID}, ActivityName={model?.name}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Save activity blocked because access context was not found",
                        action: model?.isForAdd == true ? "Create" : "Update",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"ActivityId={model?.activityID}, ActivityName={model?.name}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                if (model == null || string.IsNullOrWhiteSpace(model.name))
                {
                    AppLogger.Warn(
                        message: "Save activity request validation failed",
                        action: model?.isForAdd == true ? "Create" : "Update",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: "Activity name is required");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Activity name is required.",
                        Data = false
                    };
                }

                string actionName = model.isForAdd
                    ? ActionTypeEnum.Create.ToString().ToLower()
                    : ActionTypeEnum.Update.ToString().ToLower();

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "activity");
                bool isAllowed = permissionActions.Contains(actionName);

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Save activity blocked by permission",
                        action: model.isForAdd ? "Create" : "Update",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"ActivityId={model.activityID}, ActivityName={model.name}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                bool isSaved;

                if (model.isForAdd)
                {
                    isSaved = await _activityService.AddActivityAsync(
                        accessContext.DatabaseName,
                        accessContext.UserId,
                        model.name,
                        model.description,
                        model.color);
                }
                else
                {
                    if (model.activityID <= 0)
                    {
                        AppLogger.Warn(
                            message: "Save activity update request validation failed",
                            action: "Update",
                            result: "InvalidRequest",
                            updatedBy: accessContext.UserId,
                            description: "ActivityId is required for update");

                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Message = "Activity id is required.",
                            Data = false
                        };
                    }

                    isSaved = await _activityService.UpdateActivityAsync(
                        accessContext.DatabaseName,
                        accessContext.UserId,
                        model.activityID,
                        model.name,
                        model.description,
                        model.color);
                }

                if (!isSaved)
                {
                    AppLogger.Warn(
                        message: "Save activity failed because database operation did not complete",
                        action: model.isForAdd ? "Create" : "Update",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"ActivityId={model.activityID}, ActivityName={model.name}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to save activity.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Save activity completed successfully",
                    action: model.isForAdd ? "Create" : "Update",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"ActivityId={model.activityID}, ActivityName={model.name}");

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
                    message: "Save activity failed",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"ActivityId={model?.activityID}, ActivityName={model?.name}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "ActivityManager.SaveActivityAsync",
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

        public async Task<ApiResponse<bool>> DeleteActivityAsync(CurrentUserContext ctx, int activityId)
        {
            try
            {
                AppLogger.Info(
                    message: "Delete activity started",
                    action: "Delete",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"ActivityId={activityId}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Delete activity blocked because access context was not found",
                        action: "Delete",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"ActivityId={activityId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "activity");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Delete.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Delete activity blocked by permission",
                        action: "Delete",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"ActivityId={activityId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                if (activityId <= 0)
                {
                    AppLogger.Warn(
                        message: "Delete activity request validation failed",
                        action: "Delete",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: $"ActivityId={activityId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid activity id.",
                        Data = false
                    };
                }

                bool isDeleted = await _activityService.DeleteActivityAsync(
                    accessContext.DatabaseName,
                    activityId);

                if (!isDeleted)
                {
                    AppLogger.Warn(
                        message: "Delete activity failed because activity was not found",
                        action: "Delete",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"ActivityId={activityId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to delete activity.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Delete activity completed successfully",
                    action: "Delete",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"ActivityId={activityId}");

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
                    message: "Delete activity failed",
                    action: "Delete",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"ActivityId={activityId}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "ActivityManager.DeleteActivityAsync",
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