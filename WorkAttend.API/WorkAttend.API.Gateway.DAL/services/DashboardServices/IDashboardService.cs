using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.DashboardServices
{
    public interface IDashboardService
    {
        Task<int> GetEmployeesCountAsync(int companyId, string databaseName);
        Task<List<employeepunchhistory>> GetPunchesCountAsync(int companyId, DateTime punchTimeStart, DateTime punchTimeEnd, string databaseName);
        Task<List<resultsQuest>> GetResultsAsync(int companyId, DateTime startDate, DateTime endDate, string databaseName);
        Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage);
    }
}