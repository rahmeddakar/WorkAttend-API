using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.EmployeeServices
{
    public interface IEmployeeService
    {
        Task<List<Employees>> GetAllEmployeesDataAsync(int companyId, int departmentId, string databaseName);
        Task<List<department>> GetCompanyDepartmentsAsync(int companyId, string databaseName);
        Task<List<Jobs>> GetAllJobsAsync(string databaseName);

        Task<subscriptionpackagefeatures?> GetActivePackageDataAsync(int companyId, int featureId = 0);
        Task<List<SubscriptionPackageFeatureModel>> GetCompanySubscriptionFeaturesAsync(int companyId, int featureId = 0);
        Task<int> GetDatabaseEmployeeCountAsync(string databaseName);
        Task<employee?> GetCompanyEmployeeByEmailAsync(string email, int companyId, string databaseName);

        Task<employee?> CreateEmployeeAsync(string empEmail, string empPassword, bool isMobile, int companyId, string userId, string databaseName);
        Task<bool> CreateEmployeeProfileAsync(int employeeId, string firstName, string surName, DateTime dob, string userId, string databaseName, string employeeCode);
        Task<int> GetAttributeIdAsync(string attributeName, string databaseName);
        Task<bool> SaveProfileAttributeAsync(int attributeId, string value, int employeeId, string userId, string databaseName);
        Task<bool> AddEmployeeDepartmentAsync(int employeeId, int departmentId, string databaseName);

        Task<bool> DeleteAllEmployeeMobileAppAsync(int employeeId, string userId, string databaseName);
        Task<bool> DeleteEmployeeAsync(int employeeId, string userId, string databaseName);

        Task<bool> UpdateEmployeeDepartmentAsync(int employeeId, int departmentId, string userId, string databaseName);
        Task<bool> UpdatePasswordAsync(int employeeId, string password, string userId, string databaseName);
        Task<bool> UpdateFirstNameAsync(int employeeId, string firstName, string userId, string databaseName);
        Task<bool> UpdateSurNameAsync(int employeeId, string surName, string userId, string databaseName);
        Task<bool> UpdateEmployeeCodeAsync(int employeeId, string employeeCode, string userId, string databaseName);
        Task<bool> InsertOrUpdateEmployeeJobsAsync(int employeeId, string employeeJobIds, bool isForAdd, string userId, string databaseName);

        Task<List<Employees>> GetAllEmployeesAsync(int companyId, int departmentId, string databaseName);
        Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName);
        Task<List<CheckinResetRule>> GetCheckinResetRulesAsync(string databaseName);
        Task<int> GetCompanyCheckinResetRuleIdAsync(int companyId, string databaseName);

        Task<PetaPoco.Page<EmployeeLocationRow>> GetEmployeeLocationsPagedAsync(
            int pageIndex,
            int pageSize,
            int employeeId,
            int locationId,
            int companyId,
            string databaseName,
            string searchValue);

        Task<EmployeesLocation?> GetSingleEmployeeLocationAsync(int employeeId, int locationId, string databaseName);
        Task<bool> CreateEmployeeLocationAsync(int employeeId, int locationId, string userId, string databaseName);
        Task<bool> DeleteEmployeeLocationAsync(int employeeId, string locationName, int companyId, string userId, string databaseName);
        Task<bool> SetCompanyCheckinResetRuleIdAsync(int companyId, int ruleId, string userId, string databaseName);
    }
}