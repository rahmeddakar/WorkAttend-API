using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.BLL.InterfaceBLL
{
    public interface IEmployeesManager
    {
        Task<ApiResponse<EmployeePageData>> GetEmployeePageDataAsync(CurrentUserContext ctx, string[] departmentIds);
        Task<ApiResponse<bool>> CreateEmployeeAsync(CurrentUserContext ctx, addEmployeeModel model);
        Task<ApiResponse<bool>> DeleteEmployeeAsync(CurrentUserContext ctx, int employeeId);
        Task<ApiResponse<bool>> EditEmployeeAsync(CurrentUserContext ctx, Employees model);
        Task<ApiResponse<EmployeeLocationsPageData>> GetEmployeeLocationsPageDataAsync(CurrentUserContext ctx, string[] departmentIds);

        Task<ApiResponse<DataTableResponse<EmployeeLocationRow>>> GetEmployeeLocationsTableDataAsync(
            CurrentUserContext ctx,
            int draw,
            int start,
            int length,
            int? employeeId,
            int? locationId,
            string searchValue);
        Task<ApiResponse<CreateEmployeeLocationResponse>> CreateEmployeeLocationAsync(CurrentUserContext ctx, CreateEmployeeLocationRequest model);
        Task<ApiResponse<bool>> DeleteEmployeeLocationAsync(CurrentUserContext ctx, DeleteEmployeeLocationRequest model);
        Task<ApiResponse<bool>> SetCheckinReenableAsync(CurrentUserContext ctx, int option);
    }
}