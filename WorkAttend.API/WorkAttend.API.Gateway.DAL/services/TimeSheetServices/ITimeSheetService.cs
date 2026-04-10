using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.TimeSheetServices
{
    public interface ITimeSheetService
    {
        Task<List<Employees>> GetAllEmployeesAsync(int companyId, int departmentId, string databaseName);
        Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName);
        Task<List<timeSheet>> GetTimeSheetAsync(string databaseName, int companyId, int departmentId, DateTime startDate, DateTime endDate, int employeeId, int locationId, int punchType);
        Task<List<timeSheetEmp>> GetEmployeeTimeSheetAsync(string databaseName, int companyId, int departmentId, DateTime startDate, DateTime endDate, int employeeId, int locationId);
        Task<company?> GetCompanyByIdAsync(int companyId, string databaseName);
        Task<List<punchHistoryCSV>> GetTimeSheetForCsvAsync(int companyId, int departmentId, string startDate, string endDate, bool isIncludeDelRecords, string databaseName, int employeeId);
        Task<List<punchHistoryCoordsCSV>> GetTimeSheetForCoordsCsvAsync(int companyId, int departmentId, string startDate, string endDate, bool isIncludeDelRecords, string databaseName, int employeeId);
        Task<List<projectTimeSheet>> GetProjectTimeSheetAsync(string databaseName, int companyId, int departmentId, DateTime startDate, DateTime endDate, int employeeId, int projectId);
    }
}