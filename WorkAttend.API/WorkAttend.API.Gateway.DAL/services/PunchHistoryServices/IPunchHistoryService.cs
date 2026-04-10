using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.PunchHistoryServices
{
    public interface IPunchHistoryService
    {
        Task<List<Employees>> GetAllEmployeesAsync(int companyId, int departmentId, string databaseName);
        Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName);

        Task<List<punchTimesheetList>> GetPunchHistoryAsync(
            string startDate,
            string endDate,
            int employeeId,
            int locationId,
            int companyId,
            int departmentId,
            string databaseName,
            int pageNo,
            int pageSize = 500);

        Task<int> GetPunchHistoryCountAsync(
            string startDate,
            string endDate,
            int employeeId,
            int locationId,
            int companyId,
            int departmentId,
            string databaseName);

        Task<List<ManualPunchesModel>> GetManualPunchesAsync(string databaseName);

        Task<employeepunchhistory> CreatePunchAsync(
            string userId,
            createPunch model,
            int companyId,
            string databaseName);

        Task<company> GetBaseCompanyAsync(int companyId);
        Task<company> GetCompanyTimeZoneAsync(int companyId, string databaseName);
        Task<timezone> GetTimeZoneDetailsAsync(int timeZoneId);

        Task<employee> GetEmployeeDataAsync(int employeeId, int companyId, string databaseName);
        Task<employeeprofile> GetEmployeeProfileAsync(int employeeId, string databaseName);

        Task<punch> SavePunchToCommonDbAsync(
            int companyId,
            int employeeId,
            string dakarEmployeeCode,
            int punchHistoryId,
            int punchType,
            string peNumber,
            int locationId,
            DateTime baseTime,
            DateTime saveUtcNow,
            DateTime actualCompanyTime,
            double latitude,
            double longitude,
            string databaseName);

        Task<List<punchHistoryCSV>> GetPunchHistoryForCSVAsync(
            int companyId,
            int departmentId,
            string startDate,
            string endDate,
            bool isIncludeDelRecords,
            string databaseName);

        Task<List<punchHistoryCoordsCSV>> GetPunchHistoryForCoordsCSVAsync(
            int companyId,
            int departmentId,
            string startDate,
            string endDate,
            bool isIncludeDelRecords,
            string databaseName);

        Task<ManualPunchesModel> UpdateStatusAsync(string databaseName, bool isApproved, int recordId, string userId);

        Task<companylocation> GetCompanyFromLocationAsync(int locationId, string databaseName);
        Task<Companyconfigurations> GetCompanyFromDbNameAsync(string databaseName);

        Task<employeepunchhistory> SavePunchAsync(
            int recordId,
            int companyId,
            int employeeId,
            int punchType,
            int locationId,
            string locationCode,
            string employeeEmail,
            string createdBy,
            double latitude,
            double longitude,
            string databaseName,
            DateTime manualPunchTime,
            bool isManualPunch = false,
            string picture = null,
            string notes = null);

        Task<punchlocation> SaveExactPunchCoordinatesAsync(
            int punchHistoryId,
            double latitude,
            double longitude,
            string employeeEmail,
            string databaseName,
            int punchAttributeValueId,
            string punchAttributeValue);

        Task<company> GetCompanyFromCompIdAsync(int companyId, string databaseName);
    }
}