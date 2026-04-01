using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.CommonCode.Helpers;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentManager _departmentManager;

        public DepartmentController(IDepartmentManager departmentManager)
        {
            _departmentManager = departmentManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<List<department>>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _departmentManager.GetDepartmentsAsync(ctx);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] department model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<DepartmentOperationResult>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _departmentManager.CreateDepartmentAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDepartment([FromBody] departmentComp model)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<DepartmentOperationResult>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _departmentManager.UpdateDepartmentAsync(ctx, model);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("{departmentId:int}")]
        public async Task<IActionResult> DeleteDepartment(int departmentId)
        {
            var ctx = CurrentUserContextHelper.Get(User);
            if (ctx == null)
            {
                return Unauthorized(new ApiResponse<DepartmentOperationResult>
                {
                    Success = false,
                    Message = "Unauthorized",
                    Data = null
                });
            }

            var response = await _departmentManager.DeleteDepartmentAsync(ctx, departmentId);

            if (!response.Success)
            {
                if (response.Message == "Unauthorized")
                    return Unauthorized(response);

                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}