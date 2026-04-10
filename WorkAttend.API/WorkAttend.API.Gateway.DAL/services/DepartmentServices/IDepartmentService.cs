using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.DepartmentServices
{
    public interface IDepartmentService
    {
        Task<List<department>> GetAllDepartmentsAsync(int companyId, string databaseName);
        Task<int> GetDatabaseDepartmentCountAsync(string databaseName);
        Task<department?> CheckDepartmentExistAsync(string departmentCode, int companyId, string databaseName);
        Task<department> CreateDepartmentAsync(string departmentName, string departmentCode, string userId, string databaseName);
        Task<bool> UpdateDepartmentAsync(int departmentId, string departmentCode, string departmentName, string userId, string databaseName);
        Task<int> GetEmployeeDepartmentUsageCountAsync(string databaseName, int departmentId);
        Task<bool> DeleteDepartmentAsync(int departmentId, string userId, string databaseName);
    }
}