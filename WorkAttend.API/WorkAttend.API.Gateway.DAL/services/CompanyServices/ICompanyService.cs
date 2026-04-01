using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.CompanyServices
{
    public interface ICompanyService
    {
        Task<company> CreateCompanyAsync(string companyName, string peNumber, bool isDakarConnected, int timezoneId);
        Task<companyattribute> AddCompanyAttrAsync(int companyId, string userId, int countryId, string countryName, string phoneNum, string billedTo, string billingEmail, int currencyType, string accountType, string expectedNumOfEmp);
        Task<Companyconfigurations> CreateCompanyConfigAsync(int companyId, string databaseName, int industryId, string description, string contactNumber, string companyURL, string mobileApplicationKey);
        Task<company> CreateCompanyNewDBAsync(string companyName, string peNumber, bool isDakarConnected, string databaseName, int baseCompanyId, int timezoneId);
        Task<companydepartment> CreateCompanyDepartmentAsync(int departmentId, int companyId, string userId, string databaseName);
        Task<int> GetDatabaseCompanyCountAsync(string databaseName);
        Task<List<company>> CheckCompanyExistAsync(string peNumber);
        Task<workattendadmin?> CheckAdminExistAsync(string email, string databaseName);
        Task<company?> GetCompanyByIdAsync(int companyId, string databaseName);
        Task<companyadmin> InsertAdminCompanyAsync(int companyId, int adminId, bool isSuperAdmin, string databaseName);
        Task<string> GetAdminPolicyAsync(string databaseName);
        Task<Roles> InsertAdminRoleAsync(string databaseName, int companyId, string adminEmail);
        Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage);
    }
}