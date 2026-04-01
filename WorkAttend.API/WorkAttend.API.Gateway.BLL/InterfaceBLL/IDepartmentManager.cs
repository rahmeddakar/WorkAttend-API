using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IDepartmentManager
    {
        Task<ApiResponse<List<department>>> GetDepartmentsAsync(CurrentUserContext ctx);
        Task<ApiResponse<DepartmentOperationResult>> CreateDepartmentAsync(CurrentUserContext ctx, department model);
        Task<ApiResponse<DepartmentOperationResult>> UpdateDepartmentAsync(CurrentUserContext ctx, departmentComp model);
        Task<ApiResponse<DepartmentOperationResult>> DeleteDepartmentAsync(CurrentUserContext ctx, int departmentId);
    }
}