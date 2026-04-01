using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.Common.Helper
{
    public class HomeDataHelper
    {
        public Task<Companyconfigurations?> GetCompanyConfigurationsAsync(string companyUrl)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("companyconfigurations")
                .Where("companyURL = @0", companyUrl);

            var companyConfig = db.Fetch<Companyconfigurations>(sql).FirstOrDefault();
            return Task.FromResult(companyConfig);
        }

        public Task<int> GetAttributeIdAsync(string attributeName, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("pa.profileAttributeID")
                .From("profileattributes pa")
                .Where("pa.attributeName = @0", attributeName);

            int attributeId = db.Fetch<int>(sql).FirstOrDefault();
            return Task.FromResult(attributeId);
        }

        public Task<int> GetPunchAttributeIdAsync(string attributeName, string databaseName, int companyId)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("pa.punchAttributeID")
                .From("punchattributes pa")
                .Where("pa.name = @0 and pa.companyid = @1", attributeName, companyId);

            int attributeId = db.Fetch<int>(sql).FirstOrDefault();
            return Task.FromResult(attributeId);
        }

        public Task<List<int>> GetEmergencyOnSiteEmployeeIdsAsync(int companyId, DateTime punchTimeStart, DateTime punchTimeEnd, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("distinct ep.employeeid")
                .From("employeepunchhistory ep")
                .InnerJoin("employees e").On("e.employeeID = ep.employeeID")
                .Where(@"
                    ep.employeepunchhistoryid in
                    (
                        select max(eph.employeepunchhistoryid)
                        from employeepunchhistory eph
                        where eph.punchTime >= @0 and eph.punchTime <= @1
                        group by eph.employeeid
                    )", punchTimeStart, punchTimeEnd)
                .Where("e.isDeleted != 1 and e.companyID = @0 and ep.punchType = 1 and ep.punchTimeCountry >= @1 and ep.punchTimeCountry <= @2",
                    companyId, punchTimeStart, punchTimeEnd);

            var employeeIds = db.Fetch<int>(sql).ToList();
            return Task.FromResult(employeeIds);
        }

        public Task<List<emergencyData>> GetEmergencyListPunchesAsync(
            List<int> employeeIds,
            int mobileAttrId,
            int punchAttrId,
            int companyId,
            DateTime punchTimeStart,
            DateTime punchTimeEnd,
            string databaseName)
        {
            if (employeeIds == null || employeeIds.Count == 0)
                return Task.FromResult(new List<emergencyData>());

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("ep.employeeID, ep.employeepunchhistoryid, epf.firstname, epf.surname, e.email, ep.punchTimeCountry as punchDateTime, l.locationname, pl.punchattributevalueid as assemblypointID, pl.punchattributevalue as assemblypoint, epa.value as mobileNumber")
                .From("employeepunchhistory ep")
                .InnerJoin("employees e").On("e.employeeID = ep.employeeID")
                .InnerJoin("employeeprofile epf").On("epf.employeeID = e.employeeID")
                .InnerJoin("locations l").On("l.locationid = ep.locationid")
                .LeftJoin("punchlocation pl").On("pl.punchhistoryid = ep.employeepunchhistoryid")
                .LeftJoin("punchattributevalues pv").On("pv.punchAttributeValueID = pl.punchAttributeValueID")
                .LeftJoin("employeeprofileattributes epa").On("epa.employeeid = e.employeeid")
                .Where(@"
                    ep.employeepunchhistoryid in
                    (
                        select max(ehh.employeepunchhistoryid)
                        from employeepunchhistory ehh
                        where ehh.punchType = 1
                          and ehh.punchTimeCountry >= @1
                          and ehh.punchTimeCountry <= @2
                          and ehh.employeeid in (@3)
                        group by ehh.employeeid
                    )
                    and e.isDeleted != 1
                    and e.companyID = @0
                    and ep.punchTimeCountry >= @1
                    and ep.punchTimeCountry <= @2
                    and epa.attributeid = @4
                    and pv.punchattributeid = @5",
                    companyId, punchTimeStart, punchTimeEnd, employeeIds, mobileAttrId, punchAttrId);

            var result = db.Fetch<emergencyData>(sql).ToList();
            return Task.FromResult(result);
        }
    }
}