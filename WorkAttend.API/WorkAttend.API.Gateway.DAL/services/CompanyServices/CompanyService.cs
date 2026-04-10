using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.CompanyServices
{
    public class CompanyService : ICompanyService
    {
        public Task<company> CreateCompanyAsync(string companyName, string peNumber, bool isDakarConnected, int timezoneId)
        {
            DateTime now = DateTime.Now;

            company newCompany = new company
            {
                name = companyName,
                peNumber = peNumber,
                IsDakarConnected = isDakarConnected,
                timezoneID = timezoneId,
                createdOn = now,
                createdBy = companyName,
                updatedOn = now,
                updatedBy = companyName
            };

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var id = db.Insert(newCompany);
            newCompany.companyId = int.Parse(id.ToString()!);

            return Task.FromResult(newCompany);
        }

        public Task<companyattribute> AddCompanyAttrAsync(int companyId, string userId, int countryId, string countryName, string phoneNum, string billedTo, string billingEmail, int currencyType, string accountType, string expectedNumOfEmp)
        {
            DateTime now = DateTime.Now;

            companyattribute newCompanyAttribute = new companyattribute
            {
                companyID = companyId,
                countryID = countryId,
                country = countryName,
                phoneNumber = phoneNum,
                billedTo = billedTo,
                billingEmail = billingEmail,
                currencyID = currencyType,
                expectedAccountType = accountType,
                expectedNumrOfEmp = expectedNumOfEmp,
                createdBy = userId,
                updatedBy = userId,
                createdOn = now,
                updatedOn = now
            };

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var id = db.Insert(newCompanyAttribute);
            newCompanyAttribute.companyAttributeID = int.Parse(id.ToString()!);

            return Task.FromResult(newCompanyAttribute);
        }

        public Task<Companyconfigurations> CreateCompanyConfigAsync(int companyId, string databaseName, int industryId, string description, string contactNumber, string companyURL, string mobileApplicationKey)
        {
            DateTime now = DateTime.Now;

            Companyconfigurations newCompanyConfig = new Companyconfigurations
            {
                companyID = companyId,
                industryId = industryId,
                description = description,
                contactNumber = contactNumber,
                mobileApplicationKey = mobileApplicationKey,
                databaseName = databaseName,
                companyURL = companyURL,
                createdOn = now,
                createdBy = databaseName,
                updatedOn = now,
                updatedBy = databaseName
            };

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var id = db.Insert(newCompanyConfig);
            newCompanyConfig.companyConfigID = int.Parse(id.ToString()!);

            return Task.FromResult(newCompanyConfig);
        }

        public Task<company> CreateCompanyNewDBAsync(string companyName, string peNumber, bool isDakarConnected, string databaseName, int baseCompanyId, int timezoneId)
        {
            DateTime now = DateTime.Now;

            company newCompany = new company
            {
                baseCompanyId = baseCompanyId,
                name = companyName,
                peNumber = peNumber,
                IsDakarConnected = isDakarConnected,
                timezoneID = timezoneId,
                createdOn = now,
                createdBy = companyName,
                updatedOn = now,
                updatedBy = companyName
            };

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var id = db.Insert(newCompany);
            newCompany.companyId = int.Parse(id.ToString()!);

            return Task.FromResult(newCompany);
        }

        public Task<companydepartment> CreateCompanyDepartmentAsync(int departmentId, int companyId, string userId, string databaseName)
        {
            DateTime now = DateTime.Now;

            companydepartment newCompanyDepartment = new companydepartment
            {
                departmentID = departmentId,
                companyID = companyId,
                createdOn = now,
                createdBy = userId,
                updatedOn = now,
                updatedBy = userId
            };

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var id = db.Insert(newCompanyDepartment);
            newCompanyDepartment.companyDeptID = int.Parse(id.ToString()!);

            return Task.FromResult(newCompanyDepartment);
        }

        public Task<int> GetDatabaseCompanyCountAsync(string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("count(*)")
                .From("companies c")
                .Where("c.isDeleted != 1");

            int count = db.Fetch<int>(sql).FirstOrDefault();
            return Task.FromResult(count);
        }

        public Task<List<company>> CheckCompanyExistAsync(string peNumber)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("companies c")
                .Where("penumber = @0 and isDeleted != 1", peNumber);

            var data = db.Fetch<company>(sql).ToList();
            return Task.FromResult(data);
        }

        public Task<workattendadmin?> CheckAdminExistAsync(string email, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("admins")
                .Where("email = @0 and isDeleted != 1", email);

            var admin = db.Fetch<workattendadmin>(sql).FirstOrDefault();
            return Task.FromResult(admin);
        }

        public Task<company?> GetCompanyByIdAsync(int companyId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("companies")
                .Where("companyID = @0 and isDeleted != 1", companyId);

            var companyData = db.Fetch<company>(sql).FirstOrDefault();
            return Task.FromResult(companyData);
        }

        public Task<companyadmin> InsertAdminCompanyAsync(int companyId, int adminId, bool isSuperAdmin, string databaseName)
        {
            DateTime now = DateTime.Now;

            companyadmin newCompanyAdmin = new companyadmin
            {
                companyId = companyId,
                adminId = adminId,
                IsSuperAdmin = isSuperAdmin,
                createdOn = now,
                createdBy = adminId.ToString(),
                updatedOn = now,
                updatedBy = adminId.ToString()
            };

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            object id = db.Insert(newCompanyAdmin);
            newCompanyAdmin.companyAdminId = int.Parse(id.ToString()!);

            return Task.FromResult(newCompanyAdmin);
        }

        public Task<string> GetAdminPolicyAsync(string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("policy")
                .From("roles")
                .Where("name = @0", "Administrator");

            string policy = db.Fetch<string>(sql).FirstOrDefault();
            return Task.FromResult(policy ?? string.Empty);
        }

        public async Task<Roles> InsertAdminRoleAsync(string databaseName, int companyId, string adminEmail)
        {
            string policy = await GetAdminPolicyAsync(databaseName);
            DateTime now = DateTime.Now;

            Roles newRole = new Roles
            {
                name = "Administrator",
                description = "all responsbilities",
                policy = policy,
                companyID = companyId,
                isSystem = true,
                createdOn = now,
                createdBy = adminEmail,
                updatedOn = now,
                updatedBy = adminEmail
            };

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            object id = db.Insert(newRole);
            newRole.roleID = int.Parse(id.ToString()!);

            return newRole;
        }
    }
}