using System;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.EmployeeServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class EmployeeManager : IEmployeesManager
    {
        private readonly IEmployeeService _employeeService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        private const int DefaultDepartmentId = 1;

        public EmployeeManager(
            IEmployeeService employeeService,
            IUserAccessContextManager userAccessContextManager)
        {
            _employeeService = employeeService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<EmployeePageData>> GetEmployeePageDataAsync(CurrentUserContext ctx, string[] departmentIds)
        {
            try
            {
                AppLogger.Info(
                    message: "Employee page data load started",
                    action: "Read",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}, RawDepartmentIds={(departmentIds == null || departmentIds.Length == 0 ? "none" : string.Join(",", departmentIds))}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Employee page data load blocked because access context was not found",
                        action: "Read",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new ApiResponse<EmployeePageData>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                int normalizedDepartmentId = NormalizeDepartmentId(departmentIds);

                var employees = await _employeeService.GetAllEmployeesDataAsync(
                    accessContext.CompanyId,
                    normalizedDepartmentId,
                    accessContext.DatabaseName);

                var departments = await _employeeService.GetCompanyDepartmentsAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                var jobs = await _employeeService.GetAllJobsAsync(accessContext.DatabaseName);

                var pageData = new EmployeePageData
                {
                    employeeList = employees,
                    departments = departments,
                    employeeJobs = jobs
                };

                AppLogger.Info(
                    message: "Employee page data loaded successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"CompanyId={accessContext.CompanyId}, DepartmentId={normalizedDepartmentId}, EmployeesCount={employees.Count}, DepartmentsCount={departments.Count}, JobsCount={jobs.Count}");

                return new ApiResponse<EmployeePageData>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = pageData
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Employee page data load failed",
                    action: "Read",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeManager.GetEmployeePageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<EmployeePageData>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> CreateEmployeeAsync(CurrentUserContext ctx, addEmployeeModel model)
        {
            try
            {
                AppLogger.Info(
                    message: "Create employee started",
                    action: "Create",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"Email={model?.emailEmployee}, DepartmentId={model?.departmentID}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Create employee blocked because access context was not found",
                        action: "Create",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"Email={model?.emailEmployee}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                if (model == null ||
                    string.IsNullOrWhiteSpace(model.emailEmployee) ||
                    string.IsNullOrWhiteSpace(model.passwordEmployee) ||
                    string.IsNullOrWhiteSpace(model.firstName))
                {
                    AppLogger.Warn(
                        message: "Create employee request validation failed",
                        action: "Create",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: $"Email={model?.emailEmployee}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Please fill all fields to continue.",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "employee");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Create employee blocked by permission",
                        action: "Create",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: "Policy does not allow create on employee");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Not accessible",
                        Data = false
                    };
                }

                var companySub = await _employeeService.GetActivePackageDataAsync(accessContext.BaseCompanyId, (int)Constants.Features.Employees);
                var companySubscriptionPackageFeatureValue = await _employeeService.GetCompanySubscriptionFeaturesAsync(accessContext.BaseCompanyId, (int)Constants.Features.Employees);
                int currentEmployeeCount = await _employeeService.GetDatabaseEmployeeCountAsync(accessContext.DatabaseName);

                if (companySub == null)
                {
                    AppLogger.Warn(
                        message: "Create employee blocked because subscription information was not found",
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

                var maxEmployeeCount =
                    companySubscriptionPackageFeatureValue != null && companySubscriptionPackageFeatureValue.Count > 0
                        ? companySubscriptionPackageFeatureValue.First().FeatureValue
                        : companySub.FeatureValue;

                if (!(currentEmployeeCount < maxEmployeeCount || companySub.FeatureValue == -1))
                {
                    AppLogger.Warn(
                        message: "Create employee blocked by subscription limit",
                        action: "Create",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"CurrentEmployeeCount={currentEmployeeCount}, MaxEmployeeCount={maxEmployeeCount}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Subscribe to add more employees",
                        Data = false
                    };
                }

                var existingEmployee = await _employeeService.GetCompanyEmployeeByEmailAsync(
                    model.emailEmployee,
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                if (existingEmployee != null && existingEmployee.employeeID > 0)
                {
                    AppLogger.Warn(
                        message: "Create employee blocked because employee already exists",
                        action: "Create",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"Email={model.emailEmployee}, CompanyId={accessContext.CompanyId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Employee already exist.",
                        Data = false
                    };
                }

                int finalDepartmentId = ResolveDepartmentId(model.departmentID);

                var createdEmployee = await _employeeService.CreateEmployeeAsync(
                    model.emailEmployee,
                    model.passwordEmployee,
                    true,
                    accessContext.CompanyId,
                    accessContext.UserId,
                    accessContext.DatabaseName);

                if (createdEmployee == null || createdEmployee.employeeID <= 0)
                {
                    AppLogger.Warn(
                        message: "Create employee failed because employee record was not created",
                        action: "Create",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"Email={model.emailEmployee}, CompanyId={accessContext.CompanyId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again.",
                        Data = false
                    };
                }

                await _employeeService.CreateEmployeeProfileAsync(
                    createdEmployee.employeeID,
                    model.firstName,
                    model.surName,
                    model.dob,
                    accessContext.UserId,
                    accessContext.DatabaseName,
                    model.employeeCode);

                if (!string.IsNullOrWhiteSpace(model.mobileNumber))
                {
                    int attributeId = await _employeeService.GetAttributeIdAsync("MobileNumber", accessContext.DatabaseName);
                    if (attributeId > 0)
                    {
                        await _employeeService.SaveProfileAttributeAsync(
                            attributeId,
                            model.mobileNumber,
                            createdEmployee.employeeID,
                            accessContext.UserId,
                            accessContext.DatabaseName);
                    }
                }

                await _employeeService.AddEmployeeDepartmentAsync(
                    createdEmployee.employeeID,
                    finalDepartmentId,
                    accessContext.DatabaseName);

                bool isSmsEnabled = false;
                bool isEmailEnabled = false;

                AppLogger.Debug(
                    message: "Employee notifications skipped because they are disabled for now",
                    action: "Create",
                    result: "Skipped",
                    updatedBy: accessContext.UserId,
                    description: $"EmployeeId={createdEmployee.employeeID}, SmsEnabled={isSmsEnabled}, EmailEnabled={isEmailEnabled}");

                AppLogger.Info(
                    message: "Create employee completed successfully",
                    action: "Create",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"EmployeeId={createdEmployee.employeeID}, Email={model.emailEmployee}, CompanyId={accessContext.CompanyId}, DepartmentId={finalDepartmentId}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Employee Created successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Create employee failed",
                    action: "Create",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"Email={model?.emailEmployee}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeManager.CreateEmployeeAsync",
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

        public async Task<ApiResponse<bool>> DeleteEmployeeAsync(CurrentUserContext ctx, int employeeId)
        {
            try
            {
                AppLogger.Info(
                    message: "Delete employee started",
                    action: "Delete",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={employeeId}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Delete employee blocked because access context was not found",
                        action: "Delete",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"EmployeeId={employeeId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "employee");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Delete.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Delete employee blocked by permission",
                        action: "Delete",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"EmployeeId={employeeId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                bool deviceDeleteResult = await _employeeService.DeleteAllEmployeeMobileAppAsync(
                    employeeId,
                    accessContext.UserId,
                    accessContext.DatabaseName);

                if (!deviceDeleteResult)
                {
                    AppLogger.Warn(
                        message: "Delete employee failed because deleting mobile app records was not completed",
                        action: "Delete",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"EmployeeId={employeeId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to delete employee mobile app records.",
                        Data = false
                    };
                }

                bool isDeleted = await _employeeService.DeleteEmployeeAsync(
                    employeeId,
                    accessContext.UserId,
                    accessContext.DatabaseName);

                if (!isDeleted)
                {
                    AppLogger.Warn(
                        message: "Delete employee failed because employee record was not deleted",
                        action: "Delete",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"EmployeeId={employeeId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to delete employee.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Delete employee completed successfully",
                    action: "Delete",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"EmployeeId={employeeId}");

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
                    message: "Delete employee failed",
                    action: "Delete",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={employeeId}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeManager.DeleteEmployeeAsync",
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

        public async Task<ApiResponse<bool>> EditEmployeeAsync(CurrentUserContext ctx, Employees model)
        {
            try
            {
                AppLogger.Info(
                    message: "Edit employee started",
                    action: "Update",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={model?.employeeID}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Edit employee blocked because access context was not found",
                        action: "Update",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"EmployeeId={model?.employeeID}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                if (model == null || model.employeeID <= 0)
                {
                    AppLogger.Warn(
                        message: "Edit employee request validation failed",
                        action: "Update",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: "Model was null or employeeID was invalid");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "employee");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Update.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Edit employee blocked by permission",
                        action: "Update",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"EmployeeId={model.employeeID}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                bool isUpdatedDepartment = false;
                bool isUpdatedPassword = false;
                bool isUpdatedFirstName = false;
                bool isUpdatedLastName = false;
                bool isUpdatedEmployeeCode = false;
                bool isUpdatedEmployeeJobs = false;

                if (model.departmentID > 0)
                {
                    isUpdatedDepartment = await _employeeService.UpdateEmployeeDepartmentAsync(
                        model.employeeID,
                        model.departmentID,
                        accessContext.UserId,
                        accessContext.DatabaseName);
                }

                if (!string.IsNullOrWhiteSpace(model.password))
                {
                    isUpdatedPassword = await _employeeService.UpdatePasswordAsync(
                        model.employeeID,
                        model.password,
                        accessContext.UserId,
                        accessContext.DatabaseName);
                }

                if (!string.IsNullOrWhiteSpace(model.firstName))
                {
                    isUpdatedFirstName = await _employeeService.UpdateFirstNameAsync(
                        model.employeeID,
                        model.firstName,
                        accessContext.UserId,
                        accessContext.DatabaseName);
                }

                if (!string.IsNullOrWhiteSpace(model.surName))
                {
                    isUpdatedLastName = await _employeeService.UpdateSurNameAsync(
                        model.employeeID,
                        model.surName,
                        accessContext.UserId,
                        accessContext.DatabaseName);
                }

                isUpdatedEmployeeCode = await _employeeService.UpdateEmployeeCodeAsync(
                    model.employeeID,
                    model.employeeCode,
                    accessContext.UserId,
                    accessContext.DatabaseName);

                if (!string.IsNullOrWhiteSpace(model.employeeJobs))
                {
                    isUpdatedEmployeeJobs = await _employeeService.InsertOrUpdateEmployeeJobsAsync(
                        model.employeeID,
                        model.employeeJobs,
                        model.isForAdd,
                        accessContext.UserId,
                        accessContext.DatabaseName);
                }

                if (isUpdatedDepartment || isUpdatedPassword || isUpdatedFirstName || isUpdatedLastName || isUpdatedEmployeeCode || isUpdatedEmployeeJobs)
                {
                    AppLogger.Info(
                        message: "Edit employee completed successfully",
                        action: "Update",
                        result: "Success",
                        updatedBy: accessContext.UserId,
                        description: $"EmployeeId={model.employeeID}, UpdatedDepartment={isUpdatedDepartment}, UpdatedPassword={isUpdatedPassword}, UpdatedFirstName={isUpdatedFirstName}, UpdatedLastName={isUpdatedLastName}, UpdatedEmployeeCode={isUpdatedEmployeeCode}, UpdatedEmployeeJobs={isUpdatedEmployeeJobs}");

                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Processed successfully.",
                        Data = true
                    };
                }

                AppLogger.Warn(
                    message: "Edit employee completed with no updates",
                    action: "Update",
                    result: "Failed",
                    updatedBy: accessContext.UserId,
                    description: $"EmployeeId={model.employeeID}");

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "No changes were applied.",
                    Data = false
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Edit employee failed",
                    action: "Update",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={model?.employeeID}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeManager.EditEmployeeAsync",
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

        public async Task<ApiResponse<EmployeeLocationsPageData>> GetEmployeeLocationsPageDataAsync(CurrentUserContext ctx, string[] departmentIds)
        {
            try
            {
                AppLogger.Info(
                    message: "Employee locations page data load started",
                    action: "Read",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}, RawDepartmentIds={(departmentIds == null || departmentIds.Length == 0 ? "none" : string.Join(",", departmentIds))}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Employee locations page data load blocked because access context was not found",
                        action: "Read",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"DatabaseName={ctx.DatabaseName}");

                    return new ApiResponse<EmployeeLocationsPageData>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                int normalizedDepartmentId = NormalizeDepartmentId(departmentIds);

                var employees = await _employeeService.GetAllEmployeesAsync(
                    accessContext.CompanyId,
                    normalizedDepartmentId,
                    accessContext.DatabaseName);

                var locations = await _employeeService.GetAllLocationsAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                var checkinResetRules = await _employeeService.GetCheckinResetRulesAsync(
                    accessContext.DatabaseName);

                var selectedCheckinResetRuleId = await _employeeService.GetCompanyCheckinResetRuleIdAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                var assignEmployeesData = employees != null
                    ? employees.Select(x => new Employees
                    {
                        employeeID = x.employeeID,
                        email = x.email,
                        createdBy = x.createdBy,
                        companyID = x.companyID,
                        companyName = x.companyName,
                        firstName = x.firstName,
                        surName = x.surName,
                        dob = x.dob,
                        empDisplayName = x.empDisplayName,
                        departmentName = x.departmentName,
                        createdOn = x.createdOn,
                        manufacturerID = x.manufacturerID,
                        appVersion = x.appVersion,
                        departmentID = x.departmentID,
                        employeeCode = x.employeeCode,
                        password = x.password,
                        employeeJobs = x.employeeJobs,
                        isForAdd = x.isForAdd
                    }).ToList()
                    : new System.Collections.Generic.List<Employees>();

                assignEmployeesData.Insert(0, new Employees
                {
                    employeeID = -1,
                    empDisplayName = "All"
                });

                var pageData = new EmployeeLocationsPageData
                {
                    employeesData = employees,
                    locationsData = locations,
                    assignEmployeesData = assignEmployeesData,
                    checkinResetRules = checkinResetRules,
                    selectedCheckinResetRuleId = selectedCheckinResetRuleId
                };

                AppLogger.Info(
                    message: "Employee locations page data loaded successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"CompanyId={accessContext.CompanyId}, DepartmentId={normalizedDepartmentId}, EmployeesCount={employees.Count}, LocationsCount={locations.Count}, CheckinResetRulesCount={checkinResetRules.Count}, SelectedCheckinResetRuleId={selectedCheckinResetRuleId}");

                return new ApiResponse<EmployeeLocationsPageData>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = pageData
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Employee locations page data load failed",
                    action: "Read",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeManager.GetEmployeeLocationsPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<EmployeeLocationsPageData>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<DataTableResponse<EmployeeLocationRow>>> GetEmployeeLocationsTableDataAsync(
     CurrentUserContext ctx,
     int draw,
     int start,
     int length,
     int? employeeId,
     int? locationId,
     string searchValue)
        {
            try
            {
                AppLogger.Info(
                    message: "Employee locations table data load started",
                    action: "Read",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"Draw={draw}, Start={start}, Length={length}, EmployeeId={employeeId}, LocationId={locationId}, SearchValue={searchValue}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Employee locations table data load blocked because access context was not found",
                        action: "Read",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"Draw={draw}");

                    return new ApiResponse<DataTableResponse<EmployeeLocationRow>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                int safeLength = length > 0 ? length : 10;
                int safeStart = start >= 0 ? start : 0;
                int pageIndex = safeStart / safeLength;

                int safeEmployeeId = employeeId.GetValueOrDefault(0);
                int safeLocationId = locationId.GetValueOrDefault(0);

                var result = await _employeeService.GetEmployeeLocationsPagedAsync(
                    pageIndex,
                    safeLength,
                    safeEmployeeId,
                    safeLocationId,
                    accessContext.CompanyId,
                    accessContext.DatabaseName,
                    searchValue);

                AppLogger.Info(
                    message: "Employee locations table data loaded successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"Draw={draw}, TotalItems={result.TotalItems}, ReturnedItems={result.Items.Count}");

                return new ApiResponse<DataTableResponse<EmployeeLocationRow>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = new DataTableResponse<EmployeeLocationRow>
                    {
                        draw = draw,
                        recordsTotal = result.TotalItems,
                        recordsFiltered = result.TotalItems,
                        data = result.Items.ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Employee locations table data load failed",
                    action: "Read",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"Draw={draw}, Start={start}, Length={length}, EmployeeId={employeeId}, LocationId={locationId}, SearchValue={searchValue}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeManager.GetEmployeeLocationsTableDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<DataTableResponse<EmployeeLocationRow>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<CreateEmployeeLocationResponse>> CreateEmployeeLocationAsync(CurrentUserContext ctx, CreateEmployeeLocationRequest model)
        {
            try
            {
                AppLogger.Info(
                    message: "Create employee location started",
                    action: "Create",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={model?.employeeID}, LocationId={model?.locationID}, DepartmentId={model?.departmentID}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Create employee location blocked because access context was not found",
                        action: "Create",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"EmployeeId={model?.employeeID}, LocationId={model?.locationID}");

                    return new ApiResponse<CreateEmployeeLocationResponse>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "employee");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Create employee location blocked by permission",
                        action: "Create",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"EmployeeId={model?.employeeID}, LocationId={model?.locationID}");

                    return new ApiResponse<CreateEmployeeLocationResponse>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = new CreateEmployeeLocationResponse
                        {
                            assignCount = 0,
                            duplicateCount = 0
                        }
                    };
                }

                if (model == null || model.locationID <= 0)
                {
                    AppLogger.Warn(
                        message: "Create employee location request validation failed",
                        action: "Create",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: "LocationId was missing or invalid");

                    return new ApiResponse<CreateEmployeeLocationResponse>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = new CreateEmployeeLocationResponse
                        {
                            assignCount = 0,
                            duplicateCount = 0
                        }
                    };
                }

                if (model.employeeID > 0)
                {
                    bool isAssigned = await _employeeService.CreateEmployeeLocationAsync(
                        model.employeeID,
                        model.locationID,
                        accessContext.UserId,
                        accessContext.DatabaseName);

                    if (!isAssigned)
                    {
                        AppLogger.Warn(
                            message: "Create employee location failed because record already exists",
                            action: "Create",
                            result: "Duplicate",
                            updatedBy: accessContext.UserId,
                            description: $"EmployeeId={model.employeeID}, LocationId={model.locationID}");

                        return new ApiResponse<CreateEmployeeLocationResponse>
                        {
                            Success = false,
                            Message = "Record already exist.",
                            Data = new CreateEmployeeLocationResponse
                            {
                                assignCount = 0,
                                duplicateCount = 1
                            }
                        };
                    }

                    AppLogger.Info(
                        message: "Create employee location completed successfully",
                        action: "Create",
                        result: "Success",
                        updatedBy: accessContext.UserId,
                        description: $"EmployeeId={model.employeeID}, LocationId={model.locationID}");

                    return new ApiResponse<CreateEmployeeLocationResponse>
                    {
                        Success = true,
                        Message = "Record saved successfully.",
                        Data = new CreateEmployeeLocationResponse
                        {
                            assignCount = 1,
                            duplicateCount = 0
                        }
                    };
                }

                if (model.employeeID == -1)
                {
                    int normalizedDepartmentId = ResolveDepartmentId(model.departmentID);

                    var employees = await _employeeService.GetAllEmployeesAsync(
                        accessContext.CompanyId,
                        normalizedDepartmentId,
                        accessContext.DatabaseName);

                    int assignCount = 0;
                    int duplicateCount = 0;

                    foreach (var item in employees)
                    {
                        bool isAssigned = await _employeeService.CreateEmployeeLocationAsync(
                            item.employeeID,
                            model.locationID,
                            accessContext.UserId,
                            accessContext.DatabaseName);

                        if (isAssigned)
                            assignCount++;
                        else
                            duplicateCount++;
                    }

                    AppLogger.Info(
                        message: "Create employee location for all employees completed successfully",
                        action: "Create",
                        result: "Success",
                        updatedBy: accessContext.UserId,
                        description: $"LocationId={model.locationID}, DepartmentId={normalizedDepartmentId}, AssignCount={assignCount}, DuplicateCount={duplicateCount}");

                    return new ApiResponse<CreateEmployeeLocationResponse>
                    {
                        Success = true,
                        Message = "Processed successfully.",
                        Data = new CreateEmployeeLocationResponse
                        {
                            assignCount = assignCount,
                            duplicateCount = duplicateCount
                        }
                    };
                }

                AppLogger.Warn(
                    message: "Create employee location request validation failed",
                    action: "Create",
                    result: "InvalidRequest",
                    updatedBy: accessContext.UserId,
                    description: $"EmployeeId={model.employeeID}, LocationId={model.locationID}");

                return new ApiResponse<CreateEmployeeLocationResponse>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Data = new CreateEmployeeLocationResponse
                    {
                        assignCount = 0,
                        duplicateCount = 0
                    }
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Create employee location failed",
                    action: "Create",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={model?.employeeID}, LocationId={model?.locationID}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeManager.CreateEmployeeLocationAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<CreateEmployeeLocationResponse>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = new CreateEmployeeLocationResponse
                    {
                        assignCount = 0,
                        duplicateCount = 0
                    }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteEmployeeLocationAsync(CurrentUserContext ctx, DeleteEmployeeLocationRequest model)
        {
            try
            {
                AppLogger.Info(
                    message: "Delete employee location started",
                    action: "Delete",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={model?.employeeID}, LocationName={model?.locationName}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Delete employee location blocked because access context was not found",
                        action: "Delete",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"EmployeeId={model?.employeeID}, LocationName={model?.locationName}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "employee");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Delete.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Delete employee location blocked by permission",
                        action: "Delete",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"EmployeeId={model?.employeeID}, LocationName={model?.locationName}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                if (model == null || model.employeeID <= 0 || string.IsNullOrWhiteSpace(model.locationName))
                {
                    AppLogger.Warn(
                        message: "Delete employee location request validation failed",
                        action: "Delete",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: "EmployeeId or LocationName was invalid");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                bool isDeleted = await _employeeService.DeleteEmployeeLocationAsync(
                    model.employeeID,
                    model.locationName,
                    accessContext.CompanyId,
                    accessContext.UserId,
                    accessContext.DatabaseName);

                if (!isDeleted)
                {
                    AppLogger.Warn(
                        message: "Delete employee location failed because record was not found",
                        action: "Delete",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"EmployeeId={model.employeeID}, LocationName={model.locationName}, CompanyId={accessContext.CompanyId}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to delete employee location.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Delete employee location completed successfully",
                    action: "Delete",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"EmployeeId={model.employeeID}, LocationName={model.locationName}, CompanyId={accessContext.CompanyId}");

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
                    message: "Delete employee location failed",
                    action: "Delete",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={model?.employeeID}, LocationName={model?.locationName}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeManager.DeleteEmployeeLocationAsync",
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

        public async Task<ApiResponse<bool>> SetCheckinReenableAsync(CurrentUserContext ctx, int option)
        {
            try
            {
                AppLogger.Info(
                    message: "Set check-in re-enable started",
                    action: "Update",
                    result: "Started",
                    updatedBy: ctx.UserId,
                    description: $"Option={option}");

                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "Set check-in re-enable blocked because access context was not found",
                        action: "Update",
                        result: "Unauthorized",
                        updatedBy: ctx.UserId,
                        description: $"Option={option}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "employee");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Update.ToString().ToLower());

                if (!isAllowed)
                {
                    AppLogger.Warn(
                        message: "Set check-in re-enable blocked by permission",
                        action: "Update",
                        result: "Denied",
                        updatedBy: accessContext.UserId,
                        description: $"Option={option}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = false
                    };
                }

                if (option < 0)
                {
                    AppLogger.Warn(
                        message: "Set check-in re-enable request validation failed",
                        action: "Update",
                        result: "InvalidRequest",
                        updatedBy: accessContext.UserId,
                        description: $"Option={option}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                bool isSaved = await _employeeService.SetCompanyCheckinResetRuleIdAsync(
                    accessContext.CompanyId,
                    option,
                    accessContext.UserId,
                    accessContext.DatabaseName);

                if (!isSaved)
                {
                    AppLogger.Warn(
                        message: "Set check-in re-enable failed because value was not saved",
                        action: "Update",
                        result: "Failed",
                        updatedBy: accessContext.UserId,
                        description: $"Option={option}");

                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unable to save.",
                        Data = false
                    };
                }

                AppLogger.Info(
                    message: "Set check-in re-enable completed successfully",
                    action: "Update",
                    result: "Success",
                    updatedBy: accessContext.UserId,
                    description: $"Option={option}");

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
                    message: "Set check-in re-enable failed",
                    action: "Update",
                    result: "Failed",
                    updatedBy: ctx.UserId,
                    description: $"Option={option}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "EmployeeManager.SetCheckinReenableAsync",
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

        private int NormalizeDepartmentId(string[] departmentIds)
        {
            if (departmentIds == null || departmentIds.Length != 1)
                return DefaultDepartmentId;

            var rawValue = departmentIds[0];

            if (string.IsNullOrWhiteSpace(rawValue))
                return DefaultDepartmentId;

            if (!int.TryParse(rawValue, out int departmentId))
                return DefaultDepartmentId;

            if (departmentId <= 0)
                return DefaultDepartmentId;

            return departmentId;
        }

        private int ResolveDepartmentId(int departmentId)
        {
            return departmentId > 0 ? departmentId : DefaultDepartmentId;
        }
    }
}