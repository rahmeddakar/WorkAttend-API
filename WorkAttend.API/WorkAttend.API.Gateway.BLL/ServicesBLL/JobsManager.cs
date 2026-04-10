using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.JobsServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class JobsManager : IJobsManager
    {
        private readonly IJobsService _jobsService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public JobsManager(
            IJobsService jobsService,
            IUserAccessContextManager userAccessContextManager)
        {
            _jobsService = jobsService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<List<Jobs>>> GetJobsAsync(CurrentUserContext ctx)
        {
            try
            {
                AppLogger.Info(
                    message: "Jobs load started",
                    action: "View",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Jobs load blocked because access context was not found",
                        action: "View",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new ApiResponse<List<Jobs>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "job");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Jobs load blocked by permission",
                        action: "View",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow view on job");

                    return new ApiResponse<List<Jobs>>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var jobs = await _jobsService.GetAllJobsAsync(accessContext.DatabaseName);

                AppLogger.Info(
                    message: "Jobs load completed successfully",
                    action: "View",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"Count={jobs.Count}");

                return new ApiResponse<List<Jobs>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = jobs
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Jobs load failed",
                    action: "View",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "JobsManager.GetJobsAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<List<Jobs>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> SaveJobAsync(CurrentUserContext ctx, JobViewModel model)
        {
            try
            {
                AppLogger.Info(
                    message: "Save job started",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"JobId={model?.JobID}, JobName={model?.name}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Save job blocked because access context was not found",
                        action: model?.isForAdd == true ? "Create" : "Update",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"JobId={model?.JobID}, JobName={model?.name}");

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
                        message: "Save job request validation failed",
                        action: model?.isForAdd == true ? "Create" : "Update",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: $"JobId={model?.JobID}, JobName={model?.name}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Job name is required.",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "job");

                bool isAllowed = model.isForAdd
                    ? permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower())
                    : permissionActions.Contains(ActionTypeEnum.Update.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Save job blocked by permission",
                        action: model.isForAdd ? "Create" : "Update",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"JobId={model.JobID}, JobName={model.name}");

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
                    isSaved = await _jobsService.AddJobAsync(
                        accessContext.DatabaseName,
                        accessContext.UserId,
                        model.name);
                }
                else
                {
                    if (model.JobID <= 0)
                    {
                        AppLogger.Warn(
                            message: "Save job update request validation failed",
                            action: "Update",
                            result: "InvalidRequest",
                            updatedBy: accessContext.UserId,
                            description: $"JobId={model.JobID}, JobName={model.name}");

                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Message = "Job id is required.",
                            Data = false
                        };
                    }

                    isSaved = await _jobsService.UpdateJobAsync(
                        accessContext.DatabaseName,
                        accessContext.UserId,
                        model.JobID,
                        model.name);
                }

                if (!isSaved)
                {
                    AppLogger.Warn(
                        message: "Save job failed because database operation did not complete",
                        action: model.isForAdd ? "Create" : "Update",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"JobId={model.JobID}, JobName={model.name}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to save job.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Save job completed successfully",
                    action: model.isForAdd ? "Create" : "Update",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"JobId={model.JobID}, JobName={model.name}");

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
                    message: "Save job failed",
                    action: model?.isForAdd == true ? "Create" : "Update",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"JobId={model?.JobID}, JobName={model?.name}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "JobsManager.SaveJobAsync",
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