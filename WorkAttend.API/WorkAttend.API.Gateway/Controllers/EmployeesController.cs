using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.CommonCode.Helpers;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeesManager _employeesManager;

        public EmployeesController(IEmployeesManager employeesManager)
        {
            _employeesManager = employeesManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var rawDepartmentIds = Request.Query["departmentId"].ToArray();

                AppLogger.Info(
                    message: "Employees index request received",
                    action: "Read",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"RawDepartmentIds={(rawDepartmentIds == null || rawDepartmentIds.Length == 0 ? "none" : string.Join(",", rawDepartmentIds))}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Employees index request unauthorized because token context was not found",
                        action: "Read",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: "CurrentUserContext was null");

                    return Unauthorized(new ApiResponse<EmployeePageData>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _employeesManager.GetEmployeePageDataAsync(ctx, rawDepartmentIds);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Employees index request completed successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Employees index request failed with unexpected exception",
                    action: "Read",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"RawDepartmentIds={string.Join(",", Request.Query["departmentId"].ToArray())}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<EmployeePageData>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployee([FromBody] addEmployeeModel model)
        {
            try
            {
                AppLogger.Info(
                    message: "Create employee request received",
                    action: "Create",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"Email={model?.emailEmployee}, DepartmentId={model?.departmentID}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Create employee request unauthorized because token context was not found",
                        action: "Create",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"Email={model?.emailEmployee}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _employeesManager.CreateEmployeeAsync(ctx, model);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Create employee request completed successfully",
                    action: "Create",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}, Email={model?.emailEmployee}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Create employee request failed with unexpected exception",
                    action: "Create",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"Email={model?.emailEmployee}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong. Try again.",
                    Data = false
                });
            }
        }

        [HttpDelete("delete-employee-{employeeId:int}")]
        public async Task<IActionResult> DeleteEmployee(int employeeId)
        {
            try
            {
                AppLogger.Info(
                    message: "Delete employee request received",
                    action: "Delete",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"EmployeeId={employeeId}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Delete employee request unauthorized because token context was not found",
                        action: "Delete",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"EmployeeId={employeeId}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _employeesManager.DeleteEmployeeAsync(ctx, employeeId);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Delete employee request completed successfully",
                    action: "Delete",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}, EmployeeId={employeeId}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Delete employee request failed with unexpected exception",
                    action: "Delete",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"EmployeeId={employeeId}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                });
            }
        }

        [HttpPost("edit-employee")]
        public async Task<IActionResult> EditEmployee([FromBody] Employees model)
        {
            try
            {
                AppLogger.Info(
                    message: "Edit employee request received",
                    action: "Update",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"EmployeeId={model?.employeeID}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Edit employee request unauthorized because token context was not found",
                        action: "Update",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"EmployeeId={model?.employeeID}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _employeesManager.EditEmployeeAsync(ctx, model);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Edit employee request completed successfully",
                    action: "Update",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}, EmployeeId={model?.employeeID}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Edit employee request failed with unexpected exception",
                    action: "Update",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"EmployeeId={model?.employeeID}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                });
            }
        }

        [HttpGet("employee-locations")]
        public async Task<IActionResult> EmployeeLocations()
        {
            try
            {
                var rawDepartmentIds = Request.Query["departmentId"].ToArray();

                AppLogger.Info(
                    message: "Employee locations page request received",
                    action: "Read",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"RawDepartmentIds={(rawDepartmentIds == null || rawDepartmentIds.Length == 0 ? "none" : string.Join(",", rawDepartmentIds))}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Employee locations page request unauthorized because token context was not found",
                        action: "Read",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: "CurrentUserContext was null");

                    return Unauthorized(new ApiResponse<EmployeeLocationsPageData>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _employeesManager.GetEmployeeLocationsPageDataAsync(ctx, rawDepartmentIds);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Employee locations page request completed successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"DatabaseName={ctx.DatabaseName}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Employee locations page request failed with unexpected exception",
                    action: "Read",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"RawDepartmentIds={string.Join(",", Request.Query["departmentId"].ToArray())}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<EmployeeLocationsPageData>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                });
            }
        }

        [HttpPost("employee-locations/table-data")]
        public async Task<IActionResult> GetEmployeeLocationsTableData(
    [FromForm] int draw,
    [FromForm] int start,
    [FromForm] int length,
    [FromForm] int? employeeId,
    [FromForm] int? locationId)
        {
            try
            {
                string searchValue = Request.Form["search[value]"].ToString();

                AppLogger.Info(
                    message: "Employee locations table data request received",
                    action: "Read",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"Draw={draw}, Start={start}, Length={length}, EmployeeId={employeeId}, LocationId={locationId}, SearchValue={searchValue}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Employee locations table data request unauthorized because token context was not found",
                        action: "Read",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"Draw={draw}");

                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _employeesManager.GetEmployeeLocationsTableDataAsync(
                    ctx,
                    draw,
                    start,
                    length,
                    employeeId,
                    locationId,
                    searchValue);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                    {
                        return Unauthorized(new ApiResponse<object>
                        {
                            Success = false,
                            Message = response.Message,
                            Data = null
                        });
                    }

                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = response.Message,
                        Data = null
                    });
                }

                AppLogger.Info(
                    message: "Employee locations table data request completed successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"Draw={draw}, ReturnedRows={response.Data?.data?.Count ?? 0}");

                return Ok(response.Data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Employee locations table data request failed with unexpected exception",
                    action: "Read",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"Draw={draw}, Start={start}, Length={length}, EmployeeId={employeeId}, LocationId={locationId}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new DataTableResponse<EmployeeLocationRow>
                {
                    draw = draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new System.Collections.Generic.List<EmployeeLocationRow>()
                });
            }
        }

        [HttpPost("employee-locations/create")]
        public async Task<IActionResult> CreateEmployeeLocation([FromBody] CreateEmployeeLocationRequest model)
        {
            try
            {
                AppLogger.Info(
                    message: "Create employee location request received",
                    action: "Create",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"EmployeeId={model?.employeeID}, LocationId={model?.locationID}, DepartmentId={model?.departmentID}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Create employee location request unauthorized because token context was not found",
                        action: "Create",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"EmployeeId={model?.employeeID}, LocationId={model?.locationID}");

                    return Unauthorized(new ApiResponse<CreateEmployeeLocationResponse>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    });
                }

                var response = await _employeesManager.CreateEmployeeLocationAsync(ctx, model);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Create employee location request completed successfully",
                    action: "Create",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={model?.employeeID}, LocationId={model?.locationID}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Create employee location request failed with unexpected exception",
                    action: "Create",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"EmployeeId={model?.employeeID}, LocationId={model?.locationID}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<CreateEmployeeLocationResponse>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = new CreateEmployeeLocationResponse
                    {
                        assignCount = 0,
                        duplicateCount = 0
                    }
                });
            }
        }

        [HttpPost("employee-locations/delete")]
        public async Task<IActionResult> DeleteEmployeeLocation([FromBody] DeleteEmployeeLocationRequest model)
        {
            try
            {
                AppLogger.Info(
                    message: "Delete employee location request received",
                    action: "Delete",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"EmployeeId={model?.employeeID}, LocationName={model?.locationName}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Delete employee location request unauthorized because token context was not found",
                        action: "Delete",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"EmployeeId={model?.employeeID}, LocationName={model?.locationName}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }

                var response = await _employeesManager.DeleteEmployeeLocationAsync(ctx, model);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Delete employee location request completed successfully",
                    action: "Delete",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"EmployeeId={model?.employeeID}, LocationName={model?.locationName}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Delete employee location request failed with unexpected exception",
                    action: "Delete",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"EmployeeId={model?.employeeID}, LocationName={model?.locationName}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                });
            }
        }

        [HttpPost("employee-locations/set-checkin-reenable")]
        public async Task<IActionResult> SetCheckinReenable([FromBody] SetCheckinReenableRequest model)
        {
            try
            {
                AppLogger.Info(
                    message: "Set check-in re-enable request received",
                    action: "Update",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"Option={model?.option}");

                var ctx = CurrentUserContextHelper.Get(User);
                if (ctx == null)
                {
                    AppLogger.Warn(
                        message: "Set check-in re-enable request unauthorized because token context was not found",
                        action: "Update",
                        result: "Unauthorized",
                        updatedBy: string.Empty,
                        description: $"Option={model?.option}");

                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    });
                }
                if (model == null)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Request body is required.",
                        Data = false
                    });
                }

                var response = await _employeesManager.SetCheckinReenableAsync(ctx, model?.option ?? 0);

                if (!response.Success)
                {
                    if (response.Message == "Unauthorized")
                        return Unauthorized(response);

                    return BadRequest(response);
                }

                AppLogger.Info(
                    message: "Set check-in re-enable request completed successfully",
                    action: "Update",
                    result: "Success",
                    updatedBy: ctx.UserId,
                    description: $"Option={model?.option}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Set check-in re-enable request failed with unexpected exception",
                    action: "Update",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"Option={model?.option}",
                    exception: ex);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = false
                });
            }
        }
    }
}