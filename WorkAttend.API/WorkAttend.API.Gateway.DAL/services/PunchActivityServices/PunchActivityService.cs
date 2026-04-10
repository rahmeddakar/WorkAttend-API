using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.PunchActivityServices
{
    public class PunchActivityService : IPunchActivityService
    {
        public Task<List<punchattributes>> GetPunchAttributesAsync(int companyId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("punchattributes")
                .Where("isDeleted != 1 and companyID = @0", companyId)
                .OrderBy("punchAttributeID desc");

            return Task.FromResult(db.Fetch<punchattributes>(sql).ToList());
        }

        public Task<List<punchattributeValues>> GetPunchAttributeValuesAsync(int punchAttributeId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("punchattributevalues")
                .Where("isDeleted != 1 and punchAttributeID = @0", punchAttributeId)
                .OrderBy("punchAttributeValueID desc");

            return Task.FromResult(db.Fetch<punchattributeValues>(sql).ToList());
        }

        public Task<bool> DisableAllPunchAttributesAsync(string userId, int companyId, string databaseName)
        {
            var now = DateTime.Now;

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            db.Update<punchattributes>(
                "SET isMobileAppEnabled = 0, updatedOn = @0, updatedBy = @1 WHERE companyID = @2",
                now,
                userId,
                companyId);

            return Task.FromResult(true);
        }

        public Task<punchattributes> SavePunchAttributeAsync(
            string databaseName,
            string userId,
            int companyId,
            string name,
            string displayName,
            string description,
            bool isMobileAppEnable,
            bool isCollectDaily)
        {
            DateTime now = DateTime.Now;

            punchattributes newPunchAttribute = new punchattributes
            {
                companyID = companyId,
                name = name,
                displayName = displayName,
                description = description,
                isCollectDaily = isCollectDaily,
                isMobileAppEnabled = isMobileAppEnable,
                isDeleted = false,
                createdOn = now,
                createdBy = userId,
                updatedOn = now,
                updatedBy = userId
            };

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            object id = db.Insert(newPunchAttribute);
            newPunchAttribute.punchAttributeID = int.Parse(id.ToString()!);

            return Task.FromResult(newPunchAttribute);
        }

        public Task<punchattributeValues> SavePunchAttributeValueAsync(
            string databaseName,
            string userId,
            int attributeId,
            string value)
        {
            DateTime now = DateTime.Now;

            punchattributeValues newPunchAttributeValue = new punchattributeValues
            {
                punchAttributeID = attributeId,
                punchAttributeValue = value,
                isDeleted = false,
                createdOn = now,
                createdBy = userId,
                updatedOn = now,
                updatedBy = userId
            };

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            object id = db.Insert(newPunchAttributeValue);

            // Correctly assign the inserted row id to punchAttributeValueID
            newPunchAttributeValue.punchAttributeValueID = int.Parse(id.ToString()!);

            return Task.FromResult(newPunchAttributeValue);
        }

        public Task<bool> UpdatePunchAttributeAsync(
            int punchAttributeId,
            string userId,
            string databaseName,
            bool isMobileAppActive,
            bool isCollectDaily)
        {
            var now = DateTime.Now;

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            db.Update<punchattributes>(
                "SET isMobileAppEnabled = @0, isCollectDaily = @1, updatedOn = @2, updatedBy = @3 WHERE punchAttributeID = @4",
                isMobileAppActive,
                isCollectDaily,
                now,
                userId,
                punchAttributeId);

            return Task.FromResult(true);
        }
    }
}