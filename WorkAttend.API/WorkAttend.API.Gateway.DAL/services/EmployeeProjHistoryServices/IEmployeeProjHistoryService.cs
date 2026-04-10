using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.EmployeeProjHistoryServices
{
    public interface IEmployeeProjHistoryService
    {
        Task<List<Employees>> GetEmployeesAsync(int companyId, int departmentId, string databaseName);
        Task<List<Location>> GetLocationsAsync(int companyId, string databaseName);
    }
}