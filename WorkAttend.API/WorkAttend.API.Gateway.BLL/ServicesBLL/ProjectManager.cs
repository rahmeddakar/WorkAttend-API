using System;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.ProjectServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class ProjectManager : IProjectManager
    {
        private readonly IProjectService _projectService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public ProjectManager(
            IProjectService projectService,
            IUserAccessContextManager userAccessContextManager)
        {
            _projectService = projectService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<projectMod>> GetProjectsPageDataAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<projectMod>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "project");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<projectMod>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var projects = await _projectService.GetAllProjectsAsync(accessContext.DatabaseName);
                var locations = await _projectService.GetAllLocationsAsync(accessContext.CompanyId, accessContext.DatabaseName);

                return new ApiResponse<projectMod>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = new projectMod
                    {
                        Projects = projects,
                        Locations = locations
                    }
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "ProjectManager.GetProjectsPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<projectMod>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<projectClientList>> SaveProjectAsync(CurrentUserContext ctx, projectClientList model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<projectClientList>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "project");
                bool isAllowed = model != null && model.isForAdd
                    ? permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower())
                    : permissionActions.Contains(ActionTypeEnum.Update.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<projectClientList>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                if (model == null || string.IsNullOrWhiteSpace(model.projectName))
                {
                    return new ApiResponse<projectClientList>
                    {
                        Success = false,
                        Message = "Kindly fill all fields correctly.",
                        Data = null
                    };
                }

                int savedProjectId;

                if (model.isForAdd)
                {
                    var newProject = await _projectService.AddProjectAsync(
                        accessContext.DatabaseName,
                        accessContext.UserId,
                        model.projectName,
                        model.projectCode,
                        model.description,
                        model.locationID);

                    savedProjectId = newProject.Id;
                }
                else
                {
                    if (model.projectID <= 0)
                    {
                        return new ApiResponse<projectClientList>
                        {
                            Success = false,
                            Message = "Project id is required.",
                            Data = null
                        };
                    }

                    bool updated = await _projectService.UpdateProjectAsync(
                        accessContext.DatabaseName,
                        accessContext.UserId,
                        model.projectID,
                        model.projectName,
                        model.projectCode,
                        model.description,
                        model.locationID);

                    if (!updated)
                    {
                        return new ApiResponse<projectClientList>
                        {
                            Success = false,
                            Message = "Unable to save project.",
                            Data = null
                        };
                    }

                    savedProjectId = model.projectID;
                }

                int responseClientId = model.clientID;
                string responseClientName = model.clientName;
                string responseClientCode = model.clientCode;

                if (model.clientID == 0 &&
                    !string.IsNullOrWhiteSpace(model.clientName) &&
                    !string.IsNullOrWhiteSpace(model.clientCode))
                {
                    var newClient = await _projectService.AddClientAsync(
                        accessContext.DatabaseName,
                        accessContext.UserId,
                        accessContext.CompanyId,
                        model.clientName,
                        model.clientCode);

                    await _projectService.AddProjectClientAsync(
                        accessContext.DatabaseName,
                        accessContext.UserId,
                        savedProjectId,
                        newClient.clientID);

                    responseClientId = newClient.clientID;
                    responseClientName = newClient.name;
                    responseClientCode = newClient.code;
                }

                if (model.clientID > 0 &&
                    string.IsNullOrWhiteSpace(model.clientName) &&
                    string.IsNullOrWhiteSpace(model.clientCode))
                {
                    await _projectService.AddProjectClientAsync(
                        accessContext.DatabaseName,
                        accessContext.UserId,
                        savedProjectId,
                        model.clientID);
                }

                var savedProject = await _projectService.GetProjectByIdAsync(savedProjectId, accessContext.DatabaseName);

                return new ApiResponse<projectClientList>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = new projectClientList
                    {
                        projectID = savedProject?.Id ?? savedProjectId,
                        projectName = savedProject?.Name ?? model.projectName,
                        projectCode = savedProject?.Code ?? model.projectCode,
                        description = savedProject?.Description ?? model.description,
                        locationID = savedProject?.locationID ?? model.locationID,
                        clientID = responseClientId,
                        clientName = responseClientName,
                        clientCode = responseClientCode,
                        isForAdd = model.isForAdd
                    }
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "ProjectManager.SaveProjectAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<projectClientList>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectAsync(CurrentUserContext ctx, int projectId)
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

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "project");
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

                if (projectId <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Project id is required.",
                        Data = false
                    };
                }

                bool deleted = await _projectService.DeleteProjectAsync(accessContext.DatabaseName, projectId);

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
                    "ProjectManager.DeleteProjectAsync",
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