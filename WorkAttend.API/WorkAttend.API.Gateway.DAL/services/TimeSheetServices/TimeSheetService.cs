using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.TimeSheetServices
{
    public class TimeSheetService : ITimeSheetService
    {
        public Task<List<Employees>> GetAllEmployeesAsync(int companyId, int departmentId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            if (departmentId != 0)
            {
                var sql = Sql.Builder
                    .Select("e.employeeID, e.email, e.createdBy, d.departmentName, c.companyId, c.name as companyName, CONCAT(ep.firstname, ' ', ep.surname, ' - ', e.email) as empDisplayName, ep.firstname, ep.surname, ep.dob")
                    .From("employees e")
                    .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                    .InnerJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                    .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                    .InnerJoin("companies c").On("c.companyid = e.companyid")
                    .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0 And ed.departmentID = @1", companyId, departmentId);

                return Task.FromResult(db.Fetch<Employees>(sql).ToList());
            }
            else
            {
                var sql = Sql.Builder
                    .Select("e.employeeID, e.email, e.createdBy, d.departmentName, c.companyId, c.name as companyName, CONCAT(e.email, '-', ep.firstname, ' ', ep.surname) as empDisplayName, ep.firstname, ep.surname, ep.dob")
                    .From("employees e")
                    .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                    .LeftJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                    .LeftJoin("departments d").On("d.departmentID = ed.departmentID")
                    .InnerJoin("companies c").On("c.companyid = e.companyid")
                    .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0", companyId);

                return Task.FromResult(db.Fetch<Employees>(sql).ToList());
            }
        }

        public Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("l.locationid as Id, l.locationName as LocationName, l.locationCode as LocationCode, l.latitudeP1 as LatitudeP1, l.longitudeP1 as LongitudeP1, l.latitudeP2 as LatitudeP2, l.longitudeP2 as LongitudeP2, l.latitudeP3 as LatitudeP3, l.longitudeP3 as LongitudeP3, l.latitudeP4 as LatitudeP4, l.longitudeP4 as LongitudeP4")
                .From("companylocations cl")
                .InnerJoin("locations l").On("l.locationID = cl.locationID")
                .Where("l.isDeleted != 1 and cl.companyID = @0", companyId)
                .OrderBy("l.locationid desc");

            return Task.FromResult(db.Fetch<Location>(sql).ToList());
        }

        public Task<List<timeSheet>> GetTimeSheetAsync(string databaseName, int companyId, int departmentId, DateTime startDate, DateTime endDate, int employeeId, int locationId, int punchType)
        {
            string conditions =
                "(eh.employeeID = @0 or 0 = @0) and (eh.locationID = @1 or 0 = @1)" +
                " and (ed.departmentId = @2 or 0 = @2)" +
                " and (e.companyID = @3) and eh.punchTimeCountry >= @4" +
                " and eh.punchTimeCountry <= @5 and e.isDeleted != 1 and eh.punchType = @6";

            string minMaxFilter =
                "e.employeeid, e.email as employeeCode, CONCAT(ep.firstname, ' ', ep.SurName) as employeeName, __MIN_MAX__(eh.punchTimeCountry) as punchTimeCountry, " +
                "Date(eh.punchTimeCountry) as punchDate, eh.punchType";

            minMaxFilter = punchType == 1
                ? minMaxFilter.Replace("__MIN_MAX__", "min")
                : minMaxFilter.Replace("__MIN_MAX__", "max");

            var sql = Sql.Builder
                .Select(minMaxFilter)
                .From("employeepunchhistory eh")
                .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                .Where(conditions, employeeId, locationId, departmentId, companyId, startDate, endDate, punchType)
                .GroupBy("employeeid, employeeCode, punchDate, employeeName, punchTimeCountry, punchType, employeePunchHistoryID")
                .OrderBy("eh.employeepunchhistoryid desc");

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            return Task.FromResult(db.Fetch<timeSheet>(sql).ToList());
        }

        public Task<List<timeSheetEmp>> GetEmployeeTimeSheetAsync(string databaseName, int companyId, int departmentId, DateTime startDate, DateTime endDate, int employeeId, int locationId)
        {
            string conditions =
                "(eh.employeeID = @0 or 0 = @0) and (eh.locationID = @1 or 0 = @1)" +
                " and (ed.departmentId = @2 or 0 = @2)" +
                " and (e.companyID = @3) and eh.punchTimeCountry >= @4" +
                " and eh.punchTimeCountry <= @5 and e.isDeleted != 1";

            string selectStatement =
                "e.employeeid, e.email as employeeCode, eh.locationID, l.locationName, CONCAT(COALESCE(ep.firstname, ' ', ep.surname)) as employeeName, " +
                "Date(eh.punchTimeCountry) as punchDate, date_format(eh.punchTimeCountry, '%H:%i') as punchTime, eh.punchTimeCountry, eh.punchType, case eh.punchtype when 1 then 'In' when 2 then 'Out' end as punchTypeVal";

            var sql = Sql.Builder
                .Select(selectStatement)
                .From("employeepunchhistory eh")
                .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                .InnerJoin("locations l").On("l.locationid = eh.locationid")
                .Where(conditions, employeeId, locationId, departmentId, companyId, startDate, endDate)
                .OrderBy("eh.employeepunchhistoryid desc");

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            return Task.FromResult(db.Fetch<timeSheetEmp>(sql).ToList());
        }

        public Task<company?> GetCompanyByIdAsync(int companyId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("companies")
                .Where("companyID = @0", companyId);

            return Task.FromResult(db.Fetch<company>(sql).FirstOrDefault());
        }

        public Task<List<punchHistoryCSV>> GetTimeSheetForCsvAsync(int companyId, int departmentId, string startDate, string endDate, bool isIncludeDelRecords, string databaseName, int employeeId)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            Sql sql;

            if (isIncludeDelRecords)
            {
                if (departmentId == 0)
                {
                    sql = Sql.Builder
                        .Select("e.email as employeeCode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.companyID = @0 and eh.punchTime >= @1 and eh.punchTime <= @2 and (eh.employeeid = @3 or 0 = @3)", companyId, startDate, endDate, employeeId);
                }
                else
                {
                    sql = Sql.Builder
                        .Select("e.email as employeeCode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.companyID = @0 and ed.departmentId = @1 and eh.punchTime >= @2 and eh.punchTime <= @3 and (eh.employeeid = @4 or 0 = @4)", companyId, departmentId, startDate, endDate, employeeId);
                }
            }
            else
            {
                if (departmentId == 0)
                {
                    sql = Sql.Builder
                        .Select("e.email as employeeCode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.isDeleted != 1 and e.companyID = @0 and eh.punchTime >= @1 and eh.punchTime <= @2 and (eh.employeeid = @3 or 0 = @3)", companyId, startDate, endDate, employeeId);
                }
                else
                {
                    sql = Sql.Builder
                        .Select("e.email as employeeCode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.isDeleted != 1 and e.companyID = @0 and ed.departmentId = @1 and eh.punchTime >= @2 and eh.punchTime <= @3 and (eh.employeeid = @4 or 0 = @4)", companyId, departmentId, startDate, endDate, employeeId);
                }
            }

            return Task.FromResult(db.Fetch<punchHistoryCSV>(sql).ToList());
        }

        public Task<List<punchHistoryCoordsCSV>> GetTimeSheetForCoordsCsvAsync(int companyId, int departmentId, string startDate, string endDate, bool isIncludeDelRecords, string databaseName, int employeeId)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            Sql sql;

            if (isIncludeDelRecords)
            {
                if (departmentId == 0)
                {
                    sql = Sql.Builder
                        .Select("e.email as employeeCode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, pl.latitude, pl.longitude, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("punchlocation pl").On("pl.punchhistoryid = eh.employeepunchhistoryid")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.companyID = @0 and eh.punchTime >= @1 and eh.punchTime <= @2 and (eh.employeeid = @3 or 0 = @3)", companyId, startDate, endDate, employeeId);
                }
                else
                {
                    sql = Sql.Builder
                        .Select("e.email as employeeCode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, pl.latitude, pl.longitude, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("punchlocation pl").On("pl.punchhistoryid = eh.employeepunchhistoryid")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.companyID = @0 and ed.departmentId = @1 and eh.punchTime >= @2 and eh.punchTime <= @3 and (eh.employeeid = @4 or 0 = @4)", companyId, departmentId, startDate, endDate, employeeId);
                }
            }
            else
            {
                if (departmentId == 0)
                {
                    sql = Sql.Builder
                        .Select("e.email as employeeCode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, pl.latitude, pl.longitude, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("punchlocation pl").On("pl.punchhistoryid = eh.employeepunchhistoryid")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.isDeleted != 1 and e.companyID = @0 and eh.punchTime >= @1 and eh.punchTime <= @2 and (eh.employeeid = @3 or 0 = @3)", companyId, startDate, endDate, employeeId);
                }
                else
                {
                    sql = Sql.Builder
                        .Select("e.email as employeeCode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, pl.latitude, pl.longitude, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("punchlocation pl").On("pl.punchhistoryid = eh.employeepunchhistoryid")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.isDeleted != 1 and e.companyID = @0 and ed.departmentId = @1 and eh.punchTime >= @2 and eh.punchTime <= @3 and (eh.employeeid = @4 or 0 = @4)", companyId, departmentId, startDate, endDate, employeeId);
                }
            }

            return Task.FromResult(db.Fetch<punchHistoryCoordsCSV>(sql).ToList());
        }

        public Task<List<projectTimeSheet>> GetProjectTimeSheetAsync(string databaseName, int companyId, int departmentId, DateTime startDate, DateTime endDate, int employeeId, int projectId)
        {
            string conditions =
                "(eh.employeeID = @0 or 0 = @0)" +
                " and (ed.departmentId = @1 or 0 = @1)" +
                " and (ph.projectID = @2 or 0 = @2)" +
                " and (e.companyID = @3)" +
                " and eh.punchInTime >= @4" +
                " and eh.punchOutTime <= @5" +
                " and e.isDeleted != 1 and p.isActive = 1";

            string selectFilter =
                "e.employeeid, ph.projectID, p.name as projectName, " +
                "e.email as employeeCode, " +
                "CONCAT(COALESCE(ep.firstname, ' '), ' ', COALESCE(ep.surname, '')) as employeeName, " +
                "ph.InTime, ph.OutTime";

            var sql = Sql.Builder
                .Select(selectFilter)
                .From("projectpunchhistory ph")
                .InnerJoin("projects p").On("p.Id = ph.projectID")
                .InnerJoin("employees e").On("e.employeeID = ph.employeeID")
                .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                .InnerJoin("employeedepartment ed").On("e.employeeID = ed.employeeID")
                .Where(conditions, employeeId, departmentId, projectId, companyId, startDate, endDate)
                .OrderBy("ph.projectPunchID desc");

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            return Task.FromResult(db.Fetch<projectTimeSheet>(sql).ToList());
        }
    }
}