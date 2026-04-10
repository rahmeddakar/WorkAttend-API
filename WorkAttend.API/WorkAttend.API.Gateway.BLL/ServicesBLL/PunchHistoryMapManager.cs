using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.PunchHistoryMapServices;
using WorkAttend.API.Gateway.DAL.services.PunchHistoryServices;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class PunchHistoryMapManager : IPunchHistoryMapManager
    {
        private readonly IPunchHistoryMapService _punchHistoryMapService;
        private readonly IPunchHistoryService _punchHistoryService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public PunchHistoryMapManager(
            IPunchHistoryMapService punchHistoryMapService,
            IPunchHistoryService punchHistoryService,
            IUserAccessContextManager userAccessContextManager)
        {
            _punchHistoryMapService = punchHistoryMapService;
            _punchHistoryService = punchHistoryService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<punchHistoryMap>> GetPageDataAsync(CurrentUserContext ctx, punchHistoryMap model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<punchHistoryMap>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                model ??= new punchHistoryMap();

                int departmentId = accessContext.DepartmentId > 0 ? accessContext.DepartmentId : 0;

                model.EmployeeList = await _punchHistoryService.GetAllEmployeesAsync(
                    accessContext.CompanyId,
                    departmentId,
                    accessContext.DatabaseName) ?? new List<Employees>();

                return new ApiResponse<punchHistoryMap>
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
                    "PunchHistoryMapManager.GetPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<punchHistoryMap>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<punchHistoryMap>> GetFilterDataAsync(CurrentUserContext ctx, punchHistoryMap model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<punchHistoryMap>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                if (model == null || model.employeeID <= 0)
                {
                    return new ApiResponse<punchHistoryMap>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = null
                    };
                }

                DateTime startDate;
                DateTime endDate;

                if (string.IsNullOrWhiteSpace(model.dateFilter))
                {
                    endDate = DateTime.Today;
                    startDate = DateTime.Today.AddMonths(-12);
                }
                else
                {
                    string[] range = model.dateFilter.Split('-');
                    string startDateStr = range[0].Trim();
                    string endDateStr = range[1].Trim();

                    startDate = DateTime.ParseExact(startDateStr, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    endDate = DateTime.ParseExact(endDateStr, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                }

                var resultModel = new punchHistoryMap
                {
                    employeeID = model.employeeID,
                    dateFilter = model.dateFilter,
                    startDate = startDate,
                    endDate = endDate,
                    punchlocations = await _punchHistoryMapService.GetPunchLocationsAsync(
                        startDate,
                        endDate,
                        model.employeeID,
                        accessContext.DatabaseName) ?? new List<punchLocationMarkers>()
                };

                return new ApiResponse<punchHistoryMap>
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
                    "PunchHistoryMapManager.GetFilterDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<punchHistoryMap>
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