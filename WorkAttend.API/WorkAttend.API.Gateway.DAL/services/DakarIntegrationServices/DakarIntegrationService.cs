using PetaPoco;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.DakarIntegrationServices
{
    public class DakarIntegrationService : IDakarIntegrationService
    {
        public Task<dakarcompanyconfigs?> GetDakarConfigAsync(int baseCompanyId)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("dakarcompanyconfigs")
                .Where("baseCompanyID = @0 and isDeleted != 1", baseCompanyId)
                .OrderBy("companyConfigID desc");

            var data = db.Fetch<dakarcompanyconfigs>(sql).FirstOrDefault();
            return Task.FromResult(data);
        }

        public Task<dakarcompanyconfigs> CreateDakarCompanyConfigAsync(int baseCompanyId, string userId, string dakarURL, string companyCode, string siteCode)
        {
            DateTime now = DateTime.Now;

            dakarcompanyconfigs newCC = new dakarcompanyconfigs
            {
                baseCompanyID = baseCompanyId,
                companyId = baseCompanyId,
                appendCompanyCode = false,
                BatchCount = 2000,
                DakarURL = dakarURL,
                siteCode = siteCode,
                exportStartDate = now,
                companyCode = companyCode,
                isDeleted = false,
                createdOn = now,
                createdBy = userId,
                updatedOn = now,
                updatedBy = userId
            };

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            object id = db.Insert(newCC);
            newCC.companyConfigID = int.Parse(id.ToString()!);

            return Task.FromResult(newCC);
        }

        public Task<bool> UpdateDakarConnectedAsync(int companyId, string databaseName, string userId)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var now = DateTime.Now;

            db.Update<company>(
                "SET isDakarConnected = 1, updatedOn = @0, updatedBy = @1 WHERE companyID = @2",
                now,
                userId,
                companyId);

            return Task.FromResult(true);
        }

        public Task<bool> UpdateDakarConnectedBaseAsync(int baseCompanyId, string userId)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var now = DateTime.Now;

            db.Update<company>(
                "SET isDakarConnected = 1, updatedOn = @0, updatedBy = @1 WHERE companyID = @2",
                now,
                userId,
                baseCompanyId);

            return Task.FromResult(true);
        }

        public Task<bool> UpdateDakarURLAsync(int companyConfigId, string dakarURL, string companyCode, string siteCode, string userId)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var now = DateTime.Now;

            db.Update<dakarcompanyconfigs>(
                "SET dakarURL = @0, CompanyCode = @1, SiteCode = @2, updatedOn = @3, updatedBy = @4 WHERE companyConfigID = @5",
                dakarURL,
                companyCode,
                siteCode,
                now,
                userId,
                companyConfigId);

            return Task.FromResult(true);
        }

        public Task<bool> DeleteDakarURLAsync(int companyConfigId, string userId)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var now = DateTime.Now;

            db.Update<dakarcompanyconfigs>(
                "SET isDeleted = 1, updatedOn = @0, updatedBy = @1 WHERE companyConfigID = @2",
                now,
                userId,
                companyConfigId);

            return Task.FromResult(true);
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