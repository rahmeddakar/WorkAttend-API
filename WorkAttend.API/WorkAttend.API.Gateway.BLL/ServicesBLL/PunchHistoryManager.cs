using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.PunchHistoryServices;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class PunchHistoryManager : IPunchHistoryManager
    {
        private const string CurrentControllerName = "punchhistory";

        private readonly IPunchHistoryService _punchHistoryService;
        private readonly IUserAccessContextManager _userAccessContextManager;
        private readonly IConfiguration _configuration;

        public PunchHistoryManager(
            IPunchHistoryService punchHistoryService,
            IUserAccessContextManager userAccessContextManager,
            IConfiguration configuration)
        {
            _punchHistoryService = punchHistoryService;
            _userAccessContextManager = userAccessContextManager;
            _configuration = configuration;
        }

        public async Task<ApiResponse<Punch>> GetPunchHistoryPageDataAsync(CurrentUserContext ctx, Punch model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<Punch>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                model ??= new Punch();
                if (model.departmentID < 0)
                    model.departmentID = 0;

                model.employees = await _punchHistoryService.GetAllEmployeesAsync(
                    accessContext.CompanyId,
                    model.departmentID,
                    accessContext.DatabaseName) ?? new List<Employees>();

                model.locations = await _punchHistoryService.GetAllLocationsAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName) ?? new List<Location>();

                model.companysubscriptionpackagefeatures ??= new List<subscriptionfeature>();

                return new ApiResponse<Punch>
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
                    "PunchHistoryManager.GetPunchHistoryPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<Punch>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<Punch>> GetFilteredHistoryAsync(CurrentUserContext ctx, Punch model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<Punch>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                model ??= new Punch();

                DateTime startDate;
                DateTime endDate;

                if (string.IsNullOrWhiteSpace(model.dateFilter))
                {
                    startDate = DateTime.Today;
                    endDate = DateTime.Today;
                }
                else
                {
                    startDate = model.startDate;
                    endDate = model.endDate;
                }

                if (model.pageNo <= 0)
                    model.pageNo = 1;

                if (model.departmentID < 0)
                    model.departmentID = 0;

                model.employees = await _punchHistoryService.GetAllEmployeesAsync(
                    accessContext.CompanyId,
                    model.departmentID,
                    accessContext.DatabaseName) ?? new List<Employees>();

                model.locations = await _punchHistoryService.GetAllLocationsAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName) ?? new List<Location>();

                model.companysubscriptionpackagefeatures ??= new List<subscriptionfeature>();

                var resultModel = await BuildPunchHistoryAsync(
                    model,
                    startDate,
                    endDate,
                    model.pageNo,
                    accessContext.CompanyId,
                    model.departmentID,
                    accessContext.DatabaseName);

                return new ApiResponse<Punch>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = resultModel
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "PunchHistoryManager.GetFilteredHistoryAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<Punch>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ManualPunch>> GetFilteredManualPunchesAsync(CurrentUserContext ctx, ManualPunch model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<ManualPunch>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                model ??= new ManualPunch();

                DateTime startDate;
                DateTime endDate;

                if (string.IsNullOrWhiteSpace(model.dateFilter))
                {
                    startDate = DateTime.Today;
                    endDate = DateTime.Today;
                }
                else
                {
                    startDate = model.startDate;
                    endDate = model.endDate;
                }

                if (model.pageNo <= 0)
                    model.pageNo = 1;

                var manualPunches = await _punchHistoryService.GetManualPunchesAsync(accessContext.DatabaseName);

                model.manualpuncheslist = manualPunches ?? new List<ManualPunchesModel>();
                model.TotalRecords = model.manualpuncheslist.Count;
                model.startDate = startDate.Date;
                model.endDate = endDate.Date;
                model.dateFilter = startDate.ToString("dd/MM/yyyy") + " - " + endDate.ToString("dd/MM/yyyy");

                return new ApiResponse<ManualPunch>
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
                    "PunchHistoryManager.GetFilteredManualPunchesAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<ManualPunch>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<createPunch>> GetCreatePunchPageDataAsync(CurrentUserContext ctx, createPunch model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<createPunch>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                model ??= new createPunch();

                if (model.departmentID < 0)
                    model.departmentID = 0;

                model.punchType = model.punchType == 0 ? 1 : model.punchType;

                model.employees = await _punchHistoryService.GetAllEmployeesAsync(
                    accessContext.CompanyId,
                    model.departmentID,
                    accessContext.DatabaseName) ?? new List<Employees>();

                model.locations = await _punchHistoryService.GetAllLocationsAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName) ?? new List<Location>();

                return new ApiResponse<createPunch>
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
                    "PunchHistoryManager.GetCreatePunchPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<createPunch>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> CreatePunchAsync(CurrentUserContext ctx, createPunch model)
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

                if (!HasPermission(accessContext.Policy, CurrentControllerName, "create"))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Permission is not allowed.",
                        Data = false
                    };
                }

                if (model == null || model.employeeID <= 0 || model.locationID == 0 || model.punchType == 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                if (model.punchDate == DateTime.MinValue)
                {
                    string nowString = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                    model.punchDate = Convert.ToDateTime(nowString);
                }

                string punchDate = model.punchDate.ToString("MM/dd/yyyy HH:mm:ss");
                model.punchDate = Convert.ToDateTime(punchDate);

                employeepunchhistory punchHistoryResult = await _punchHistoryService.CreatePunchAsync(
                    accessContext.UserId.ToString(),
                    model,
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                if (punchHistoryResult != null && punchHistoryResult.employeePunchHistoryID > 0)
                {
                    var companyConfig = await _punchHistoryService.GetCompanyFromDbNameAsync(accessContext.DatabaseName);
                    if (companyConfig != null)
                    {
                        company companyData = await _punchHistoryService.GetBaseCompanyAsync(companyConfig.companyID);

                        if (companyData != null && companyData.companyId > 0 && companyData.IsDakarConnected)
                        {
                            try
                            {
                                TimeZoneInfo infoTimeOrig = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                                DateTime baseTime = TimeZoneInfo.ConvertTime(model.punchDate, infoTimeOrig);
                                DateTime baseTimeUtc = TimeZoneInfo.ConvertTimeToUtc(model.punchDate);

                                DateTime actualCompanyTime = DateTime.MinValue;

                                company companyObj = await _punchHistoryService.GetCompanyTimeZoneAsync(
                                    accessContext.CompanyId,
                                    accessContext.DatabaseName);

                                if (companyObj != null && companyObj.timezoneID > 0)
                                {
                                    timezone companyTimeZone = await _punchHistoryService.GetTimeZoneDetailsAsync(companyObj.timezoneID);
                                    if (companyTimeZone != null && companyTimeZone.timezoneID > 0)
                                    {
                                        TimeZoneInfo infoTime = TimeZoneInfo.FindSystemTimeZoneById(companyTimeZone.name);
                                        actualCompanyTime = TimeZoneInfo.ConvertTime(baseTimeUtc, infoTime);
                                    }
                                }

                                employee emp = await _punchHistoryService.GetEmployeeDataAsync(
                                    model.employeeID,
                                    accessContext.CompanyId,
                                    accessContext.DatabaseName);

                                employeeprofile empProfile = await _punchHistoryService.GetEmployeeProfileAsync(
                                    model.employeeID,
                                    accessContext.DatabaseName);

                                string dakarEmpCode = string.Empty;

                                if (empProfile != null && !string.IsNullOrWhiteSpace(empProfile.employeeCode))
                                    dakarEmpCode = empProfile.employeeCode;
                                else
                                    dakarEmpCode = emp?.email ?? string.Empty;

                                await _punchHistoryService.SavePunchToCommonDbAsync(
                                    companyData.companyId,
                                    model.employeeID,
                                    dakarEmpCode,
                                    punchHistoryResult.employeePunchHistoryID,
                                    model.punchType,
                                    companyData.peNumber,
                                    model.locationID,
                                    model.punchDate,
                                    baseTimeUtc,
                                    actualCompanyTime,
                                    0,
                                    0,
                                    accessContext.DatabaseName);
                            }
                            catch
                            {
                                // preserve old behavior: swallow Dakar sync exception
                            }
                        }
                    }
                }

                return new ApiResponse<bool>
                {
                    Success = punchHistoryResult != null && punchHistoryResult.employeePunchHistoryID > 0,
                    Message = "Processed successfully.",
                    Data = punchHistoryResult != null && punchHistoryResult.employeePunchHistoryID > 0
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "PunchHistoryManager.CreatePunchAsync",
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

        public async Task<ApiResponse<PunchHistoryExportFile>> ExportPunchHistoryAsync(CurrentUserContext ctx, PunchHistoryExportRequest model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<PunchHistoryExportFile>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                if (!HasPermission(accessContext.Policy, CurrentControllerName, "export"))
                {
                    return new ApiResponse<PunchHistoryExportFile>
                    {
                        Success = false,
                        Message = "Permission is not allowed.",
                        Data = null
                    };
                }

                if (model == null)
                {
                    return new ApiResponse<PunchHistoryExportFile>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = null
                    };
                }

                int departmentId = model.DepartmentID < 0 ? 0 : model.DepartmentID;

                TimeSpan startTime = new TimeSpan(0, 0, 0);
                TimeSpan endTime = new TimeSpan(23, 59, 59);

                DateTime sD = model.FromDate.Date + startTime;
                string startDate = sD.ToString("yyyy-MM-dd HH:mm:ss");

                DateTime eD = model.ToDate.Date + endTime;
                string endDate = eD.ToString("yyyy-MM-dd HH:mm:ss");

                company currentCompany = await _punchHistoryService.GetCompanyFromCompIdAsync(
                    accessContext.CompanyId,
                    accessContext.DatabaseName);

                if (model.IsIncludeCoords)
                {
                    List<punchHistoryCoordsCSV> data = await _punchHistoryService.GetPunchHistoryForCoordsCSVAsync(
                        accessContext.CompanyId,
                        departmentId,
                        startDate,
                        endDate,
                        model.IsIncludeDelRecords,
                        accessContext.DatabaseName);

                    if (data == null || data.Count == 0)
                    {
                        return new ApiResponse<PunchHistoryExportFile>
                        {
                            Success = false,
                            Message = "No data is available.",
                            Data = null
                        };
                    }

                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("employee code, name, surname, department Name, department Code, location ID, locationCode, locationName, punchdate, punchtime, In/Out, location/manual, company, company PE Number, latitude, longitude, employee Unique Code");

                    foreach (var item in data)
                    {
                        sb.AppendLine(
                            item.employeeCode + ", " +
                            item.firstname + ", " +
                            item.surname + ", " +
                            item.departmentName + ", " +
                            item.departmentCode + ", " +
                            item.locationID + ", " +
                            HandleNullValue(item.locationCode).Replace(",", " ") + ", " +
                            HandleNullValue(item.locationName).Replace(",", " ") + ", " +
                            item.punchdate + ", " +
                            item.punchtime + ", " +
                            item.punchType + ", " +
                            item.locormanual + ", " +
                            (currentCompany?.name ?? "") + ", " +
                            (currentCompany?.peNumber ?? "") + ", " +
                            item.latitude + ", " +
                            item.longitude + ", " +
                            item.employeeUniqueIdentity);
                    }

                    return new ApiResponse<PunchHistoryExportFile>
                    {
                        Success = true,
                        Message = "Processed successfully.",
                        Data = new PunchHistoryExportFile
                        {
                            HasData = true,
                            FileBytes = new System.Text.UTF8Encoding().GetBytes(sb.ToString()),
                            FileName = "punches_export_" + DateTime.Now.ToString("yyyyMMMdd_HHmmss") + ".csv",
                            ContentType = "text/csv"
                        }
                    };
                }
                else
                {
                    List<punchHistoryCSV> data = await _punchHistoryService.GetPunchHistoryForCSVAsync(
                        accessContext.CompanyId,
                        departmentId,
                        startDate,
                        endDate,
                        model.IsIncludeDelRecords,
                        accessContext.DatabaseName);

                    if (data == null || data.Count == 0)
                    {
                        return new ApiResponse<PunchHistoryExportFile>
                        {
                            Success = false,
                            Message = "No data is available.",
                            Data = null
                        };
                    }

                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("employee code, name, surname, department Name, department Code, location ID, locationCode, locationName, punchdate, punchtime, In/Out, location/manual, company, company PE Number, employee unique code");

                    foreach (var item in data)
                    {
                        sb.AppendLine(
                            item.employeeCode + ", " +
                            item.firstname + ", " +
                            item.surname + ", " +
                            item.departmentName + ", " +
                            item.departmentCode + ", " +
                            item.locationID + ", " +
                            HandleNullValue(item.locationCode).Replace(",", " ") + ", " +
                            HandleNullValue(item.locationName).Replace(",", " ") + ", " +
                            item.punchdate + ", " +
                            item.punchtime + ", " +
                            item.punchType + ", " +
                            item.locormanual + ", " +
                            (currentCompany?.name ?? "") + ", " +
                            (currentCompany?.peNumber ?? "") + ", " +
                            item.employeeUniqueIdentity);
                    }

                    return new ApiResponse<PunchHistoryExportFile>
                    {
                        Success = true,
                        Message = "Processed successfully.",
                        Data = new PunchHistoryExportFile
                        {
                            HasData = true,
                            FileBytes = new System.Text.UTF8Encoding().GetBytes(sb.ToString()),
                            FileName = "export.csv",
                            ContentType = "text/csv"
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "PunchHistoryManager.ExportPunchHistoryAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<PunchHistoryExportFile>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> ApproveRejectManualPunchesAsync(CurrentUserContext ctx, ManualPunchApprovalRequest model)
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

                if (model == null || model.Id <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                var result = await _punchHistoryService.UpdateStatusAsync(
                    accessContext.DatabaseName,
                    model.Status,
                    model.Id,
                    accessContext.UserId.ToString());

                if (result == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Record not found.",
                        Data = false
                    };
                }

                if (model.Status)
                {
                    employeepunchhistory employeePunchHistory = await _punchHistoryService.SavePunchAsync(
                        model.Id,
                        accessContext.CompanyId,
                        result.EmployeeId,
                        result.punchType,
                        result.LocationId,
                        result.LocationCode,
                        result.EmployeeEmail,
                        result.EmployeeEmail,
                        result.Lattitude,
                        result.Longitude,
                        accessContext.DatabaseName,
                        result.PunchTime,
                        true,
                        null,
                        null);

                    if (employeePunchHistory != null && employeePunchHistory.employeePunchHistoryID > 0)
                    {
                        await _punchHistoryService.SaveExactPunchCoordinatesAsync(
                            employeePunchHistory.employeePunchHistoryID,
                            result.Lattitude,
                            result.Longitude,
                            result.EmployeeEmail,
                            accessContext.DatabaseName,
                            0,
                            null);
                    }
                }

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "PunchHistoryManager.ApproveRejectManualPunchesAsync",
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

        private async Task<Punch> BuildPunchHistoryAsync(
            Punch model,
            DateTime startDateParam,
            DateTime endDateParam,
            int pageNo,
            int companyId,
            int departmentId,
            string databaseName)
        {
            TimeSpan tsStart = new TimeSpan(0, 0, 0);
            TimeSpan tsEnd = new TimeSpan(23, 59, 59);

            DateTime startDate = startDateParam.Date + tsStart;
            DateTime endDate = endDateParam.Date + tsEnd;

            string startDateString = startDate.ToString("yyyy-MM-dd HH:mm:ss");
            string endDateString = endDate.ToString("yyyy-MM-dd HH:mm:ss");

            int totalRecords = await _punchHistoryService.GetPunchHistoryCountAsync(
                startDateString,
                endDateString,
                model.employeeID,
                model.locationID,
                companyId,
                departmentId,
                databaseName);

            List<punchTimesheetList> punchRecords = await _punchHistoryService.GetPunchHistoryAsync(
                startDateString,
                endDateString,
                model.employeeID,
                model.locationID,
                companyId,
                departmentId,
                databaseName,
                pageNo);

            var groupedPunches = punchRecords
                .GroupBy(p => new { p.employeeID, p.locationID, Date = p.punchTimeCountry.Date })
                .ToList();

            List<PunchDetail> punchDetails = new List<PunchDetail>();
            string baseUrl = _configuration["mobileAppBaseUrl"] ?? string.Empty;

            foreach (var group in groupedPunches)
            {
                var punches = group.OrderBy(p => p.punchTimeCountry).ToList();
                var punchArray = new Dictionary<string, SinglePunch>();
                TimeSpan total = new TimeSpan();

                int punchCount = punches.Count;
                List<int> traversed = new List<int>();

                for (int i = 0; i < punchCount; i++)
                {
                    if (traversed.Contains(i))
                        continue;

                    var punch = punches[i];
                    SinglePunch sp = new SinglePunch();

                    if (punch.punchType == 1)
                    {
                        sp.timeIn = punch.punchTimeCountry.ToString("dd/MM/yyyy HH:mm:ss");
                        sp.latitudeIn = punch.latitude.ToString();
                        sp.longitudeIn = punch.longitude.ToString();
                        sp.punchHistoryInId = punch.employeePunchHistoryID;
                        sp.pictureIn = BuildImageUrl(baseUrl, punch.picture);
                        sp.notesIn = !string.IsNullOrEmpty(punch.notes) ? punch.notes : "";

                        int outIndex = -1;
                        for (int k = i + 1; k < punchCount; k++)
                        {
                            if (punches[k].punchType == 2 && !traversed.Contains(k))
                            {
                                outIndex = k;
                                break;
                            }
                        }

                        if (outIndex > i)
                        {
                            var outPunch = punches[outIndex];

                            sp.timeOut = outPunch.punchTimeCountry.ToString("dd/MM/yyyy HH:mm:ss");
                            sp.latitudeOut = outPunch.latitude.ToString();
                            sp.longitudeOut = outPunch.longitude.ToString();
                            sp.punchHistoryOutId = outPunch.employeePunchHistoryID;
                            sp.pictureOut = BuildImageUrl(baseUrl, outPunch.picture);
                            sp.notesOut = !string.IsNullOrEmpty(outPunch.notes) ? outPunch.notes : "";

                            TimeSpan diff = outPunch.punchTimeCountry - punch.punchTimeCountry;
                            sp.ActualHours = string.Format("{0:00}:{1:00}:{2:00}", diff.Hours, diff.Minutes, diff.Seconds);

                            total = total.Add(diff);
                            traversed.Add(outIndex);
                        }
                        else
                        {
                            sp.timeOut = "Missing Punch";
                            sp.latitudeOut = "";
                            sp.longitudeOut = "";
                            sp.pictureOut = BuildImageUrl(baseUrl, null);
                            sp.ActualHours = "00:00:00";
                        }

                        traversed.Add(i);
                        punchArray.Add(i.ToString(), sp);
                    }
                    else if (punch.punchType == 2)
                    {
                        sp.timeIn = "Missing Punch";
                        sp.latitudeIn = "";
                        sp.longitudeIn = "";
                        sp.pictureIn = BuildImageUrl(baseUrl, null);

                        sp.timeOut = punch.punchTimeCountry.ToString("dd/MM/yyyy HH:mm:ss");
                        sp.latitudeOut = punch.latitude.ToString();
                        sp.longitudeOut = punch.longitude.ToString();
                        sp.punchHistoryOutId = punch.employeePunchHistoryID;
                        sp.pictureOut = BuildImageUrl(baseUrl, punch.picture);
                        sp.ActualHours = "00:00:00";

                        traversed.Add(i);
                        punchArray.Add(i.ToString(), sp);
                    }
                }

                Employees emp = model.employees.FirstOrDefault(e => e.employeeID == group.Key.employeeID);
                Location loc = model.locations.FirstOrDefault(l => l.Id == group.Key.locationID);

                string employeeName = emp != null ? $"{emp.firstName} {emp.surName}" : "";
                string locationName = loc != null ? loc.LocationName : "";
                string dayName = group.Key.Date.DayOfWeek.ToString();

                PunchDetail detail = new PunchDetail
                {
                    punchId = punches.First().employeePunchHistoryID,
                    employeeID = group.Key.employeeID,
                    employeeName = employeeName,
                    locationId = group.Key.locationID,
                    locationName = locationName,
                    punchInOutDate = group.Key.Date.ToString("yyyy/MM/dd"),
                    punchInOut = punchArray,
                    totalHours = string.Format("{0:00}:{1:00}:{2:00}", total.Hours, total.Minutes, total.Seconds),
                    dayName = dayName
                };

                punchDetails.Add(detail);
            }

            model.punchDetails = punchDetails.OrderByDescending(x => x.punchInOutDate).ToList();
            model.TotalRecords = totalRecords;
            model.startDate = startDate.Date + tsStart;
            model.endDate = endDate.Date + tsStart;
            model.dateFilter = startDate.ToString("dd/MM/yyyy") + " - " + endDate.ToString("dd/MM/yyyy");

            return model;
        }

        private async Task<UserAccessContext?> ResolveAccessContextAsync(CurrentUserContext ctx)
        {
            return await _userAccessContextManager.GetAsync(ctx);
        }

        private static bool HasPermission(string policy, string controllerName, string actionName)
        {
            if (string.IsNullOrWhiteSpace(policy) || string.IsNullOrWhiteSpace(controllerName))
                return false;

            try
            {
                JObject policyJson = JObject.Parse(policy);
                List<string> actionsAllowed = new List<string>();

                if (policyJson.ContainsKey(controllerName))
                {
                    foreach (var item in policyJson[controllerName])
                    {
                        actionsAllowed.Add(item.ToString().ToLower());
                    }
                }

                return actionsAllowed.Contains(actionName.ToLower());
            }
            catch
            {
                return false;
            }
        }

        private static string HandleNullValue(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        private static string BuildImageUrl(string baseUrl, string picturePath)
        {
            string root = (baseUrl ?? string.Empty).TrimEnd('/');

            if (string.IsNullOrWhiteSpace(picturePath))
                return root + "/Uploads/no-image.png";

            return root + "/" + picturePath.TrimStart('~', '/');
        }
    }
}