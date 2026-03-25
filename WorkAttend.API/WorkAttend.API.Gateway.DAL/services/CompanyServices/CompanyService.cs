using System;
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

        public Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage)
        {
            DateTime now = DateTime.Now;

            appexception newException = new appexception
            {
                source = source,
                message = message,
                originatedAt = originatedAt,
                stacktrace = stackTrace,
                innerexceptionmessage = innerExceptionMessage,
                createdOn = now,
                createdBy = "system",
                updatedOn = now,
                updatedBy = "system"
            };

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            db.Insert(newException);

            return Task.CompletedTask;
        }
    }
}