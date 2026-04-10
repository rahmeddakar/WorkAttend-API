using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.TimeSheetServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class TimeSheetManager : ITimeSheetManager
    {
        private readonly ITimeSheetService _timeSheetService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public TimeSheetManager(
            ITimeSheetService timeSheetService,
            IUserAccessContextManager userAccessContextManager)
        {
            _timeSheetService = timeSheetService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<TimeSheetPageData>> GetPageDataAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<TimeSheetPageData>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "timesheet");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<TimeSheetPageData>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                var employees = await _timeSheetService.GetAllEmployeesAsync(accessContext.CompanyId, accessContext.DepartmentId, accessContext.DatabaseName);
                var locations = await _timeSheetService.GetAllLocationsAsync(accessContext.CompanyId, accessContext.DatabaseName);

                return new ApiResponse<TimeSheetPageData>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = new TimeSheetPageData
                    {
                        employees = employees,
                        locations = locations
                    }
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "TimeSheetManager.GetPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<TimeSheetPageData>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<timeSheetPunchList>> GetTimeSheetAsync(CurrentUserContext ctx, timeSheet model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<timeSheetPunchList>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "timesheet");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<timeSheetPunchList>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                model ??= new timeSheet();

                TimeSpan tsStart = new TimeSpan(0, 0, 0);
                TimeSpan tsEnd = new TimeSpan(23, 59, 59);

                if (model.startDateFilter.Year == 1)
                {
                    model.startDateFilter = DateTime.Today.AddDays(-7);
                    model.endDateFilter = DateTime.Today;
                }

                DateTime startDate = model.startDateFilter.Date + tsStart;
                DateTime endDate = model.endDateFilter.Date + tsEnd;

                List<timeSheet> timeSheetIn = await _timeSheetService.GetTimeSheetAsync(
                    accessContext.DatabaseName,
                    accessContext.CompanyId,
                    accessContext.DepartmentId,
                    startDate,
                    endDate,
                    model.employeeid,
                    model.locationID,
                    1);

                List<timeSheet> timeSheetOut = await _timeSheetService.GetTimeSheetAsync(
                    accessContext.DatabaseName,
                    accessContext.CompanyId,
                    accessContext.DepartmentId,
                    startDate,
                    endDate,
                    model.employeeid,
                    model.locationID,
                    2);

                List<timeSheetPunch> timeSheetPunch = new List<timeSheetPunch>();

                employeePunches temp;

                foreach (var item in timeSheetIn)
                {
                    timeSheetPunch obj = timeSheetPunch.Find(e => e.employeeID == item.employeeid);
                    if (obj == null)
                    {
                        obj = new timeSheetPunch
                        {
                            employeeID = item.employeeid,
                            employeeName = item.employeeName,
                            employeePunchesList = new List<employeePunches>()
                        };

                        temp = new employeePunches
                        {
                            punchDate = item.punchDate.ToString("yyyy-MM-dd"),
                            punchIn = item.punchTimeCountry.ToString("yyyy-MM-dd HH:mm:ss")
                        };

                        obj.employeePunchesList.Add(temp);
                        timeSheetPunch.Add(obj);
                    }
                    else
                    {
                        temp = new employeePunches
                        {
                            punchDate = item.punchDate.ToString("yyyy-MM-dd"),
                            punchIn = item.punchTimeCountry.ToString("yyyy-MM-dd HH:mm:ss")
                        };

                        obj.employeePunchesList.Add(temp);
                    }
                }

                foreach (var item in timeSheetOut)
                {
                    timeSheetPunch obj = timeSheetPunch.Find(e => e.employeeID == item.employeeid);

                    if (obj == null)
                    {
                        obj = new timeSheetPunch
                        {
                            employeeID = item.employeeid,
                            employeeName = item.employeeName,
                            employeePunchesList = new List<employeePunches>()
                        };

                        temp = new employeePunches
                        {
                            punchDate = item.punchDate.ToString("yyyy-MM-dd"),
                            punchOut = item.punchTimeCountry.ToString("yyyy-MM-dd HH:mm:ss")
                        };

                        obj.employeePunchesList.Add(temp);
                        timeSheetPunch.Add(obj);
                    }
                    else
                    {
                        temp = obj.employeePunchesList.Find(e => e.punchDate == item.punchDate.ToString("yyyy-MM-dd"));

                        if (temp == null)
                        {
                            temp = new employeePunches
                            {
                                punchDate = item.punchDate.ToString("yyyy-MM-dd"),
                                punchOut = item.punchTimeCountry.ToString("yyyy-MM-dd HH:mm:ss")
                            };

                            obj.employeePunchesList.Add(temp);
                        }
                        else
                        {
                            temp.punchOut = item.punchTimeCountry.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                }

                return new ApiResponse<timeSheetPunchList>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = new timeSheetPunchList
                    {
                        punchTimeSheetList = timeSheetPunch
                    }
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "TimeSheetManager.GetTimeSheetAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<timeSheetPunchList>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<List<timeSheetEmp>>> GetEmployeeTimeSheetAsync(CurrentUserContext ctx, timeSheetEmployee model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<List<timeSheetEmp>>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "timesheet");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<List<timeSheetEmp>>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                model ??= new timeSheetEmployee();

                TimeSpan tsStart = new TimeSpan(0, 0, 0);
                TimeSpan tsEnd = new TimeSpan(23, 59, 59);

                if (model.startDateFilter.Year == 1)
                {
                    model.startDateFilter = DateTime.Today;
                    model.endDateFilter = DateTime.Today;
                }

                DateTime startDate = model.startDateFilter.Date + tsStart;
                DateTime endDate = model.endDateFilter.Date + tsEnd;

                var data = await _timeSheetService.GetEmployeeTimeSheetAsync(
                    accessContext.DatabaseName,
                    accessContext.CompanyId,
                    accessContext.DepartmentId,
                    startDate,
                    endDate,
                    model.employeeid,
                    model.locationID);

                return new ApiResponse<List<timeSheetEmp>>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "TimeSheetManager.GetEmployeeTimeSheetAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<List<timeSheetEmp>>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<TimeSheetCsvExportResult>> ExportTimeSheetCsvAsync(CurrentUserContext ctx, TimeSheetExportRequest model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<TimeSheetCsvExportResult>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "timesheet");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Export.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<TimeSheetCsvExportResult>
                    {
                        Success = false,
                        Message = "Permission is not allowed.",
                        Data = null
                    };
                }

                if (model == null)
                {
                    return new ApiResponse<TimeSheetCsvExportResult>
                    {
                        Success = false,
                        Message = "Invalid export request.",
                        Data = null
                    };
                }

                TimeSpan startTime = new TimeSpan(0, 0, 0);
                TimeSpan endTime = new TimeSpan(23, 59, 59);

                DateTime sD = model.fromDate.Date + startTime;
                string startDate = sD.ToString("yyyy-MM-dd HH:mm:ss");

                DateTime eD = model.toDate.Date + endTime;
                string endDate = eD.ToString("yyyy-MM-dd HH:mm:ss");

                company currentCompany = await _timeSheetService.GetCompanyByIdAsync(accessContext.CompanyId, accessContext.DatabaseName);
                if (currentCompany == null)
                {
                    return new ApiResponse<TimeSheetCsvExportResult>
                    {
                        Success = false,
                        Message = "Company not found.",
                        Data = null
                    };
                }

                var sb = new StringBuilder();

                if (model.isIncludeCoords)
                {
                    List<punchHistoryCoordsCSV> empHistory = await _timeSheetService.GetTimeSheetForCoordsCsvAsync(
                        accessContext.CompanyId,
                        accessContext.DepartmentId,
                        startDate,
                        endDate,
                        model.isIncludeDelRecords,
                        accessContext.DatabaseName,
                        model.employeeID);

                    if (empHistory.Count == 0)
                    {
                        return new ApiResponse<TimeSheetCsvExportResult>
                        {
                            Success = false,
                            Message = "no data is avaiable.",
                            Data = null
                        };
                    }

                    sb.AppendLine("employee code, name, surname, department Name, department Code, punchdate, punchtime, In/Out, location/manual, company, company PE Number, latitude, longitude");

                    foreach (var data in empHistory)
                    {
                        sb.AppendLine(
                            $"{data.employeeCode}, {data.firstname}, {data.surname}, {data.departmentName}, {data.departmentCode}, {data.punchdate}, {data.punchtime}, {data.punchType}, {data.locormanual}, {currentCompany.name}, {currentCompany.peNumber}, {data.latitude}, {data.longitude}");
                    }
                }
                else
                {
                    List<punchHistoryCSV> empHistory = await _timeSheetService.GetTimeSheetForCsvAsync(
                        accessContext.CompanyId,
                        accessContext.DepartmentId,
                        startDate,
                        endDate,
                        model.isIncludeDelRecords,
                        accessContext.DatabaseName,
                        model.employeeID);

                    if (empHistory.Count == 0)
                    {
                        return new ApiResponse<TimeSheetCsvExportResult>
                        {
                            Success = false,
                            Message = "no data is avaiable.",
                            Data = null
                        };
                    }

                    sb.AppendLine("employee code, name, surname, department Name, department Code, punchdate, punchtime, In/Out, location/manual, company, company PE Number");

                    foreach (var data in empHistory)
                    {
                        sb.AppendLine(
                            $"{data.employeeCode}, {data.firstname}, {data.surname}, {data.departmentName}, {data.departmentCode}, {data.punchdate}, {data.punchtime}, {data.punchType}, {data.locormanual}, {currentCompany.name}, {currentCompany.peNumber}");
                    }
                }

                return new ApiResponse<TimeSheetCsvExportResult>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = new TimeSheetCsvExportResult
                    {
                        fileBytes = Encoding.UTF8.GetBytes(sb.ToString()),
                        contentType = "text/csv",
                        fileName = "export.csv"
                    }
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "TimeSheetManager.ExportTimeSheetCsvAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<TimeSheetCsvExportResult>
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

        public async Task<ApiResponse<projectTimeSheetList>> GetProjectTimeSheetAsync(CurrentUserContext ctx, projectTimeSheet model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<projectTimeSheetList>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "timesheet");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.View.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<projectTimeSheetList>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                model ??= new projectTimeSheet();

                var projectTs = await _timeSheetService.GetProjectTimeSheetAsync(
                    accessContext.DatabaseName,
                    accessContext.CompanyId,
                    accessContext.DepartmentId,
                    model.startDateFilter,
                    model.endDateFilter,
                    model.employeeid,
                    model.projectID);

                var locations = await _timeSheetService.GetAllLocationsAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                var employees = await _timeSheetService.GetAllEmployeesAsync(
                    accessContext.CompanyId,
                    accessContext.DepartmentId,
                    accessContext.DatabaseName);

                return new ApiResponse<projectTimeSheetList>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = new projectTimeSheetList
                    {
                        projectTsList = projectTs,
                        employees = employees,
                        locations = locations,
                        employeeID = model.employeeid
                    }
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "TimeSheetManager.GetProjectTimeSheetAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<projectTimeSheetList>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }
    }
}