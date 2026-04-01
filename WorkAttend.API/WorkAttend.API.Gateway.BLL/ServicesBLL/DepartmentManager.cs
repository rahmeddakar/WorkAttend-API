using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.services.AdminsServices;
using WorkAttend.API.Gateway.DAL.services.CompanyServices;
using WorkAttend.API.Gateway.DAL.services.DepartmentServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class DepartmentManager : IDepartmentManager
    {
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;
        private readonly IAdminsService _adminsService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public DepartmentManager(
            IDepartmentService departmentService,
            ICompanyService companyService,
            IAdminsService adminsService,
            IUserAccessContextManager userAccessContextManager)
        {
            _departmentService = departmentService;
            _companyService = companyService;
            _adminsService = adminsService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<List<department>>> GetDepartmentsAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<List<department>>
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
                    return new ApiResponse<List<department>>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var data = await _departmentService.GetAllDepartmentsAsync(accessContext.CompanyId, accessContext.DatabaseName);

                return new ApiResponse<List<department>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                await _departmentService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "DepartmentManager.GetDepartmentsAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<List<department>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<DepartmentOperationResult>> CreateDepartmentAsync(CurrentUserContext ctx, department model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var companySub = await _adminsService.GetActivePackageData(accessContext.BaseCompanyId, (int)Constants.Features.Departments);
                int currentDepartmentCount = await _departmentService.GetDatabaseDepartmentCountAsync(accessContext.DatabaseName);

                if (companySub == null)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Subscription information not found.",
                        Data = null
                    };
                }

                if (!(currentDepartmentCount < companySub.FeatureValue || companySub.FeatureValue == -1))
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Subscription limit reached.",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "newcompany");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "You do not have permission to create department.",
                        Data = null
                    };
                }

                if (model == null || string.IsNullOrWhiteSpace(model.departmentName) || string.IsNullOrWhiteSpace(model.departmentCode))
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Kindly fill all fields correctly.",
                        Data = null
                    };
                }

                var existingDepartment = await _departmentService.CheckDepartmentExistAsync(model.departmentCode, accessContext.CompanyId, accessContext.DatabaseName);
                if (existingDepartment != null && existingDepartment.departmentID > 0)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = true,
                        Message = string.Empty,
                        Data = new DepartmentOperationResult
                        {
                            isExist = true,
                            isCreated = false
                        }
                    };
                }

                var newDepartment = await _departmentService.CreateDepartmentAsync(
                    model.departmentName,
                    model.departmentCode,
                    accessContext.UserId,
                    accessContext.DatabaseName);

                var companyDepartment = await _companyService.CreateCompanyDepartmentAsync(
                    newDepartment.departmentID,
                    accessContext.CompanyId,
                    accessContext.UserId,
                    accessContext.DatabaseName);

                bool created = companyDepartment != null && companyDepartment.companyDeptID > 0;

                return new ApiResponse<DepartmentOperationResult>
                {
                    Success = true,
                    Message = created ? string.Empty : "Something went wrong.",
                    Data = new DepartmentOperationResult
                    {
                        isExist = false,
                        isCreated = created
                    }
                };
            }
            catch (Exception ex)
            {
                await _departmentService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "DepartmentManager.CreateDepartmentAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<DepartmentOperationResult>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<DepartmentOperationResult>> UpdateDepartmentAsync(CurrentUserContext ctx, departmentComp model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "newcompany");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Update.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                if (model == null || model.departmentID <= 0)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Something went wrong.",
                        Data = null
                    };
                }

                bool updated = await _departmentService.UpdateDepartmentAsync(
                    model.departmentID,
                    model.departmentCode,
                    model.departmentName,
                    accessContext.UserId,
                    accessContext.DatabaseName);

                return new ApiResponse<DepartmentOperationResult>
                {
                    Success = true,
                    Message = string.Empty,
                    Data = new DepartmentOperationResult
                    {
                        isUpdated = updated
                    }
                };
            }
            catch (Exception ex)
            {
                await _departmentService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "DepartmentManager.UpdateDepartmentAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<DepartmentOperationResult>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<DepartmentOperationResult>> DeleteDepartmentAsync(CurrentUserContext ctx, int departmentId)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "newcompany");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Delete.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                if (departmentId <= 0)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = false,
                        Message = "Something went wrong.",
                        Data = null
                    };
                }

                int deptCount = await _departmentService.GetEmployeeDepartmentUsageCountAsync(accessContext.DatabaseName, departmentId);

                if (deptCount > 0)
                {
                    return new ApiResponse<DepartmentOperationResult>
                    {
                        Success = true,
                        Message = string.Empty,
                        Data = new DepartmentOperationResult
                        {
                            isInUse = true,
                            isDeleted = false
                        }
                    };
                }

                bool deleted = await _departmentService.DeleteDepartmentAsync(departmentId, accessContext.UserId, accessContext.DatabaseName);

                return new ApiResponse<DepartmentOperationResult>
                {
                    Success = true,
                    Message = string.Empty,
                    Data = new DepartmentOperationResult
                    {
                        isInUse = false,
                        isDeleted = deleted
                    }
                };
            }
            catch (Exception ex)
            {
                await _departmentService.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "DepartmentManager.DeleteDepartmentAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<DepartmentOperationResult>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        private async Task<UserAccessContext?> ResolveAccessContextAsync(CurrentUserContext ctx)
        {
            return await _userAccessContextManager.GetAsync(ctx);
        }
    }
}