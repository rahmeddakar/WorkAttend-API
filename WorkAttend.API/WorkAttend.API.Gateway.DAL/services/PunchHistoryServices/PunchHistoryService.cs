using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.PunchHistoryServices
{
    public class PunchHistoryService : IPunchHistoryService
    {
        public async Task<List<Employees>> GetAllEmployeesAsync(int companyId, int departmentId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            if (departmentId != 0)
            {
                var ppSql = Sql.Builder
                    .Select("e.employeeID, e.email, e.createdBy, d.departmentName, c.companyId, c.name as companyName, CONCAT(ep.firstname, ' ', ep.surname, ' - ', e.email) as empDisplayName, ep.firstname, ep.surname, ep.dob")
                    .From("employees e")
                    .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                    .InnerJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                    .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                    .InnerJoin("companies c").On("c.companyid = e.companyid")
                    .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0 And ed.departmentID = @1", companyId, departmentId);

                return await Task.FromResult(context.Fetch<Employees>(ppSql).ToList());
            }
            else
            {
                var ppSql = Sql.Builder
                    .Select("e.employeeID, e.email, e.createdBy, d.departmentName, c.companyId, c.name as companyName, CONCAT(e.email, '-', ep.firstname, ' ', ep.surname) as empDisplayName, ep.firstname, ep.surname, ep.dob")
                    .From("employees e")
                    .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                    .LeftJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                    .LeftJoin("departments d").On("d.departmentID = ed.departmentID")
                    .InnerJoin("companies c").On("c.companyid = e.companyid")
                    .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0", companyId);

                return await Task.FromResult(context.Fetch<Employees>(ppSql).ToList());
            }
        }

        public async Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("l.locationid, l.locationName, l.locationCode, l.latitudeP1, l.longitudeP1, l.latitudeP2, l.longitudeP2, l.latitudeP3, l.longitudeP3, l.latitudeP4, l.longitudeP4")
                .From("companylocations cl")
                .InnerJoin("locations l").On("l.locationID = cl.locationID")
                .Where("l.isDeleted != 1 and cl.companyID = @0", companyId)
                .OrderBy("l.locationid desc");

            return await Task.FromResult(context.Fetch<Location>(ppSql).ToList());
        }

        public async Task<List<punchTimesheetList>> GetPunchHistoryAsync(
            string startDate,
            string endDate,
            int employeeId,
            int locationId,
            int companyId,
            int departmentId,
            string databaseName,
            int pageNo,
            int pageSize = 500)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            Sql ppSql;

            if (departmentId == 0)
            {
                ppSql = Sql.Builder
                    .Select("eh.*, pl.longitude, pl.latitude")
                    .From("employeepunchhistory eh")
                    .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                    .LeftJoin("punchlocation pl").On("eh.employeepunchhistoryid = pl.punchhistoryid")
                    .Where("eh.punchTimeCountry >= @0 and eh.punchTimeCountry <= @1 and eh.locationID != 0 and e.companyID = @2 and e.isdeleted != 1",
                        startDate, endDate, companyId)
                    .OrderBy("eh.employeepunchhistoryid desc");

                if (employeeId > 0 && locationId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("eh.*, pl.longitude, pl.latitude")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .LeftJoin("punchlocation pl").On("eh.employeepunchhistoryid = pl.punchhistoryid")
                        .Where("eh.employeeID = @0 and eh.locationID = @1 and eh.punchTimeCountry >= @2 and eh.punchTimeCountry <= @3 and eh.locationID != 0 and e.companyID = @4 and e.isdeleted != 1",
                            employeeId, locationId, startDate, endDate, companyId)
                        .OrderBy("eh.employeepunchhistoryid desc");
                }
                else if (employeeId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("eh.*, pl.longitude, pl.latitude")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .LeftJoin("punchlocation pl").On("eh.employeepunchhistoryid = pl.punchhistoryid")
                        .Where("eh.employeeID = @0 and eh.locationID != 0 and eh.punchTimeCountry >= @1 and eh.punchTimeCountry <= @2 and e.companyID = @3 and e.isdeleted != 1",
                            employeeId, startDate, endDate, companyId)
                        .OrderBy("eh.employeepunchhistoryid desc");
                }
                else if (locationId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("eh.*, pl.longitude, pl.latitude")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .LeftJoin("punchlocation pl").On("eh.employeepunchhistoryid = pl.punchhistoryid")
                        .Where("eh.locationID = @0 and eh.punchTimeCountry >= @1 and eh.punchTimeCountry <= @2 and e.companyID = @3 and e.isdeleted != 1",
                            locationId, startDate, endDate, companyId)
                        .OrderBy("eh.employeepunchhistoryid desc");
                }
            }
            else
            {
                ppSql = Sql.Builder
                    .Select("eh.*, pl.longitude, pl.latitude")
                    .From("employeepunchhistory eh")
                    .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                    .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                    .LeftJoin("punchlocation pl").On("eh.employeepunchhistoryid = pl.punchhistoryid")
                    .Where("eh.punchTimeCountry >= @0 and eh.punchTimeCountry <= @1 and eh.locationID != 0 and e.companyID = @2 and ed.departmentId = @3 and e.isdeleted != 1",
                        startDate, endDate, companyId, departmentId)
                    .OrderBy("eh.employeepunchhistoryid desc");

                if (employeeId > 0 && locationId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("eh.*, pl.longitude, pl.latitude")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                        .LeftJoin("punchlocation pl").On("eh.employeepunchhistoryid = pl.punchhistoryid")
                        .Where("eh.employeeID = @0 and eh.locationID = @1 and eh.punchTimeCountry >= @2 and eh.punchTimeCountry <= @3 and eh.locationID != 0 and e.companyID = @4 and ed.departmentId = @5 and e.isdeleted != 1",
                            employeeId, locationId, startDate, endDate, companyId, departmentId)
                        .OrderBy("eh.employeepunchhistoryid desc");
                }
                else if (employeeId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("eh.*, pl.longitude, pl.latitude")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                        .LeftJoin("punchlocation pl").On("eh.employeepunchhistoryid = pl.punchhistoryid")
                        .Where("eh.employeeID = @0 and eh.locationID != 0 and eh.punchTimeCountry >= @1 and eh.punchTimeCountry <= @2 and e.companyID = @3 and ed.departmentId = @4 and e.isdeleted != 1",
                            employeeId, startDate, endDate, companyId, departmentId)
                        .OrderBy("eh.employeepunchhistoryid desc");
                }
                else if (locationId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("eh.*, pl.longitude, pl.latitude")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                        .LeftJoin("punchlocation pl").On("eh.employeepunchhistoryid = pl.punchhistoryid")
                        .Where("eh.locationID = @0 and eh.punchTimeCountry >= @1 and eh.punchTimeCountry <= @2 and e.companyID = @3 and ed.departmentId = @4 and e.isdeleted != 1",
                            locationId, startDate, endDate, companyId, departmentId)
                        .OrderBy("eh.employeepunchhistoryid desc");
                }
            }

            ppSql = ppSql.Append($" LIMIT {pageSize}");
            ppSql = ppSql.Append($" OFFSET {(pageNo - 1) * pageSize}");

            return await Task.FromResult(context.Fetch<punchTimesheetList>(ppSql).ToList());
        }

        public async Task<int> GetPunchHistoryCountAsync(
            string startDate,
            string endDate,
            int employeeId,
            int locationId,
            int companyId,
            int departmentId,
            string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            Sql ppSql;

            if (departmentId == 0)
            {
                ppSql = Sql.Builder
                    .Select("COUNT(*)")
                    .From("employeepunchhistory eh")
                    .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                    .Where("eh.punchTimeCountry >= @0 and eh.punchTimeCountry <= @1 and eh.locationID != 0 and e.companyID = @2 and e.isdeleted != 1",
                        startDate, endDate, companyId);

                if (employeeId > 0 && locationId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("COUNT(*)")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .Where("eh.employeeID = @0 and eh.locationID = @1 and eh.punchTimeCountry >= @2 and eh.punchTimeCountry <= @3 and eh.locationID != 0 and e.companyID = @4 and e.isdeleted != 1",
                            employeeId, locationId, startDate, endDate, companyId);
                }
                else if (employeeId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("COUNT(*)")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .Where("eh.employeeID = @0 and eh.locationID != 0 and eh.punchTimeCountry >= @1 and eh.punchTimeCountry <= @2 and e.companyID = @3 and e.isdeleted != 1",
                            employeeId, startDate, endDate, companyId);
                }
                else if (locationId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("COUNT(*)")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .Where("eh.locationID = @0 and eh.punchTimeCountry >= @1 and eh.punchTimeCountry <= @2 and e.companyID = @3 and e.isdeleted != 1",
                            locationId, startDate, endDate, companyId);
                }
            }
            else
            {
                ppSql = Sql.Builder
                    .Select("COUNT(*)")
                    .From("employeepunchhistory eh")
                    .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                    .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                    .Where("eh.punchTimeCountry >= @0 and eh.punchTimeCountry <= @1 and eh.locationID != 0 and e.companyID = @2 and ed.departmentId = @3 and e.isdeleted != 1",
                        startDate, endDate, companyId, departmentId);

                if (employeeId > 0 && locationId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("COUNT(*)")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                        .Where("eh.employeeID = @0 and eh.locationID = @1 and eh.punchTimeCountry >= @2 and eh.punchTimeCountry <= @3 and eh.locationID != 0 and e.companyID = @4 and ed.departmentId = @5 and e.isdeleted != 1",
                            employeeId, locationId, startDate, endDate, companyId, departmentId);
                }
                else if (employeeId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("COUNT(*)")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                        .Where("eh.employeeID = @0 and eh.locationID != 0 and eh.punchTimeCountry >= @1 and eh.punchTimeCountry <= @2 and e.companyID = @3 and ed.departmentId = @4 and e.isdeleted != 1",
                            employeeId, startDate, endDate, companyId, departmentId);
                }
                else if (locationId > 0)
                {
                    ppSql = Sql.Builder
                        .Select("COUNT(*)")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeedepartment ed").On("eh.employeeID = ed.employeeID")
                        .Where("eh.locationID = @0 and eh.punchTimeCountry >= @1 and eh.punchTimeCountry <= @2 and e.companyID = @3 and ed.departmentId = @4 and e.isdeleted != 1",
                            locationId, startDate, endDate, companyId, departmentId);
                }
            }

            return await Task.FromResult(context.Fetch<int>(ppSql).FirstOrDefault());
        }

        public async Task<List<ManualPunchesModel>> GetManualPunchesAsync(string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("m.Id, ep.FirstName, ep.SurName, l.locationName, m.Reason, m.PunchTime, m.punchType, m.isApproved")
                .From("manualpunches m")
                .InnerJoin("employeeprofile ep").On("ep.employeeId = m.employeeId")
                .InnerJoin("locations l").On("m.locationId = l.locationId")
                .Where("m.isApproved = 0 and l.isDeleted = 0");

            return await Task.FromResult(context.Fetch<ManualPunchesModel>(ppSql).ToList());
        }

        public async Task<employeepunchhistory> CreatePunchAsync(
            string userId,
            createPunch model,
            int companyId,
            string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var punchObj = new employeepunchhistory
            {
                employeeID = model.employeeID,
                locationID = model.locationID,
                punchType = model.punchType,
                punchTime = model.punchDate,
                punchTimeUTC = TimeZoneInfo.ConvertTimeToUtc(model.punchDate),
                punchTimeCountry = model.punchDate,
                createdBy = userId,
                updatedBy = userId,
                createdOn = DateTime.Now,
                updatedOn = DateTime.Now
            };

            object id = context.Insert(punchObj);
            punchObj.employeePunchHistoryID = Convert.ToInt32(id);

            return await Task.FromResult(punchObj);
        }

        public async Task<company> GetBaseCompanyAsync(int companyId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("companies")
                .Where("companyID = @0", companyId);

            return await Task.FromResult(context.Fetch<company>(ppSql).FirstOrDefault());
        }

        public async Task<company> GetCompanyTimeZoneAsync(int companyId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("companies")
                .Where("companyID = @0", companyId);

            return await Task.FromResult(context.Fetch<company>(ppSql).FirstOrDefault());
        }

        public async Task<timezone> GetTimeZoneDetailsAsync(int timeZoneId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("timezones")
                .Where("timezoneID = @0", timeZoneId);

            return await Task.FromResult(context.Fetch<timezone>(ppSql).FirstOrDefault());
        }

        public async Task<employee> GetEmployeeDataAsync(int employeeId, int companyId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("employees e")
                .Where("e.isDeleted != 1 And e.employeeID = @0 and e.companyID = @1", employeeId, companyId);

            return await Task.FromResult(context.Fetch<employee>(ppSql).FirstOrDefault());
        }

        public async Task<employeeprofile> GetEmployeeProfileAsync(int employeeId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("employeeprofile")
                .Where("employeeid = @0", employeeId);

            return await Task.FromResult(context.Fetch<employeeprofile>(ppSql).FirstOrDefault());
        }

        public async Task<punch> SavePunchToCommonDbAsync(
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
            string databaseName)
        {
            return await SavePunchToCommonDbInternalAsync(
                companyId,
                employeeId,
                dakarEmployeeCode,
                punchHistoryId,
                punchType,
                peNumber,
                locationId,
                string.Empty,
                baseTime,
                saveUtcNow,
                actualCompanyTime,
                latitude,
                longitude);
        }

        public async Task<List<punchHistoryCSV>> GetPunchHistoryForCSVAsync(
            int companyId,
            int departmentId,
            string startDate,
            string endDate,
            bool isIncludeDelRecords,
            string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            if (isIncludeDelRecords)
            {
                if (departmentId == 0)
                {
                    var ppSql = Sql.Builder
                        .Select("e.email as employeecode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.companyID = @0 and eh.punchTime >= @1 and eh.punchTime <= @2",
                            companyId, startDate, endDate);

                    return await Task.FromResult(context.Fetch<punchHistoryCSV>(ppSql).ToList());
                }
                else
                {
                    var ppSql = Sql.Builder
                        .Select("e.email as employeecode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.companyID = @0 and ed.departmentId = @1 and eh.punchTime >= @2 and eh.punchTime <= @3",
                            companyId, departmentId, startDate, endDate);

                    return await Task.FromResult(context.Fetch<punchHistoryCSV>(ppSql).ToList());
                }
            }
            else
            {
                if (departmentId == 0)
                {
                    var ppSql = Sql.Builder
                        .Select("e.email as employeecode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.isDeleted != 1 and e.companyID = @0 and eh.punchTime >= @1 and eh.punchTime <= @2",
                            companyId, startDate, endDate);

                    return await Task.FromResult(context.Fetch<punchHistoryCSV>(ppSql).ToList());
                }
                else
                {
                    var ppSql = Sql.Builder
                        .Select("e.email as employeecode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.isDeleted != 1 and e.companyID = @0 and ed.departmentId = @1 and eh.punchTime >= @2 and eh.punchTime <= @3",
                            companyId, departmentId, startDate, endDate);

                    return await Task.FromResult(context.Fetch<punchHistoryCSV>(ppSql).ToList());
                }
            }
        }

        public async Task<List<punchHistoryCoordsCSV>> GetPunchHistoryForCoordsCSVAsync(
            int companyId,
            int departmentId,
            string startDate,
            string endDate,
            bool isIncludeDelRecords,
            string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            if (isIncludeDelRecords)
            {
                if (departmentId == 0)
                {
                    var ppSql = Sql.Builder
                        .Select("e.email as employeecode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, pl.latitude, pl.longitude, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("punchlocation pl").On("pl.punchhistoryid = eh.employeepunchhistoryid")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.companyID = @0 and eh.punchTime >= @1 and eh.punchTime <= @2",
                            companyId, startDate, endDate);

                    return await Task.FromResult(context.Fetch<punchHistoryCoordsCSV>(ppSql).ToList());
                }
                else
                {
                    var ppSql = Sql.Builder
                        .Select("e.email as employeecode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, pl.latitude, pl.longitude, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("punchlocation pl").On("pl.punchhistoryid = eh.employeepunchhistoryid")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.companyID = @0 and ed.departmentId = @1 and eh.punchTime >= @2 and eh.punchTime <= @3",
                            companyId, departmentId, startDate, endDate);

                    return await Task.FromResult(context.Fetch<punchHistoryCoordsCSV>(ppSql).ToList());
                }
            }
            else
            {
                if (departmentId == 0)
                {
                    var ppSql = Sql.Builder
                        .Select("e.email as employeecode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, pl.latitude, pl.longitude, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("punchlocation pl").On("pl.punchhistoryid = eh.employeepunchhistoryid")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .Where("eh.locationID != 0 and e.isDeleted != 1 and e.companyID = @0 and eh.punchTime >= @1 and eh.punchTime <= @2",
                            companyId, startDate, endDate);

                    return await Task.FromResult(context.Fetch<punchHistoryCoordsCSV>(ppSql).ToList());
                }
                else
                {
                    var ppSql = Sql.Builder
                        .Select("e.email as employeecode, ep.firstname, ep.surname, d.departmentName, d.departmentCode, eh.locationID, loc.locationCode, loc.locationName, DATE_FORMAT(eh.punchTimeCountry, '%d/%m/%Y') as punchdate, DATE_FORMAT(eh.punchTimeCountry, '%H:%i:%s') as punchtime, case eh.punchtype when 1 then 'I' when 2 then 'O' end as punchType, case eh.createdby when 'mobileapp' then 'manual' else 'location' end as locormanual, pl.latitude, pl.longitude, ep.employeeCode as employeeUniqueIdentity")
                        .From("employeepunchhistory eh")
                        .InnerJoin("employees e").On("e.employeeID = eh.employeeID")
                        .InnerJoin("employeeprofile ep").On("ep.employeeID = e.employeeID")
                        .InnerJoin("employeedepartment ed").On("ed.employeeID = e.employeeID")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("locations loc").On("eh.locationID = loc.locationID")
                        .LeftJoin("punchlocation pl").On("pl.punchhistoryid = eh.employeepunchhistoryid")
                        .Where("eh.locationID != 0 and e.isDeleted != 1 and e.companyID = @0 and ed.departmentId = @1 and eh.punchTime >= @2 and eh.punchTime <= @3",
                            companyId, departmentId, startDate, endDate);

                    return await Task.FromResult(context.Fetch<punchHistoryCoordsCSV>(ppSql).ToList());
                }
            }
        }

        public async Task<ManualPunchesModel> UpdateStatusAsync(string databaseName, bool isApproved, int recordId, string userId)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var manualPunchToUpdate = new manualpunches
            {
                id = recordId,
                isApproved = isApproved,
                updatedOn = DateTime.Now,
                updatedBy = userId
            };

            context.Update(manualPunchToUpdate);

            var ppSql = Sql.Builder
                .Select("e.employeeId as EmployeeId, e.email as EmployeeEmail, m.locationId as LocationId, l.locationCode as LocationCode, m.PunchType as punchType, m.PunchTime, l.latitudeP1 as Lattitude, l.longitudeP1 as Longitude")
                .From("manualpunches m")
                .InnerJoin("locations l").On("m.locationId = l.locationId")
                .InnerJoin("employees e").On("e.employeeId = m.employeeId")
                .Where("m.Id = @0 and l.isDeleted = 0 and e.isDeleted = 0", recordId);

            return await Task.FromResult(context.Fetch<ManualPunchesModel>(ppSql).FirstOrDefault());
        }

        public async Task<companylocation> GetCompanyFromLocationAsync(int locationId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("companylocations")
                .Where("locationID = @0", locationId);

            return await Task.FromResult(context.Fetch<companylocation>(ppSql).FirstOrDefault());
        }

        public async Task<Companyconfigurations> GetCompanyFromDbNameAsync(string databaseName)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("companyconfigurations")
                .Where("databasename = @0", databaseName);

            return await Task.FromResult(context.Fetch<Companyconfigurations>(ppSql).FirstOrDefault());
        }

        public async Task<employeepunchhistory> SavePunchAsync(
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
            string notes = null)
        {
            companylocation locationObj = await GetCompanyFromLocationAsync(locationId, databaseName);

            DateTime now = DateTime.Now;
            TimeZoneInfo infoTimeOrig = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            DateTime baseTime = TimeZoneInfo.ConvertTime(now, infoTimeOrig);
            DateTime saveUtcNow = DateTime.UtcNow;

            DateTime actualCompanyTime = DateTime.MinValue;
            company companyObj = null;

            if (locationObj != null)
            {
                int locationCompanyId = locationObj.companyID;

                if (locationCompanyId > 0)
                {
                    companyObj = await GetCompanyTimeZoneAsync(locationCompanyId, databaseName);

                    if (companyObj != null && companyObj.timezoneID > 0)
                    {
                        timezone companyTimeZone = await GetCompanyTimeZoneDetailsFromCompanyDbAsync(companyObj.timezoneID, databaseName);

                        if (companyTimeZone != null && companyTimeZone.timezoneID > 0)
                        {
                            try
                            {
                                TimeZoneInfo infoTime = TimeZoneInfo.FindSystemTimeZoneById(companyTimeZone.name);
                                actualCompanyTime = TimeZoneInfo.ConvertTime(saveUtcNow, infoTime);
                            }
                            catch
                            {
                                actualCompanyTime = DateTime.UtcNow;
                            }
                        }
                    }
                }
            }

            employeepunchhistory punchObj = new employeepunchhistory
            {
                employeeID = employeeId,
                punchTime = manualPunchTime,
                punchTimeUTC = saveUtcNow,
                punchTimeCountry = isManualPunch ? manualPunchTime : actualCompanyTime,
                punchType = punchType,
                locationID = locationId,
                createdBy = createdBy,
                updatedBy = createdBy,
                createdOn = DateTime.Now,
                updatedOn = DateTime.Now,
                ismanualpunch = isManualPunch,
                manualpunchId = recordId
            };

            if (picture != null)
                punchObj.picture = picture;

            if (notes != null)
                punchObj.notes = notes;

            var context = DataContextHelper.GetCompanyDataContext(databaseName);
            
                object id = context.Insert(punchObj);
                punchObj.employeePunchHistoryID = Convert.ToInt32(id);
            

            Companyconfigurations cf = await GetCompanyFromDbNameAsync(databaseName);
            company baseComp = cf != null ? await GetBaseCompanyAsync(cf.companyID) : null;

            if (baseComp != null && baseComp.companyId > 0 && baseComp.IsDakarConnected)
            {
                try
                {
                    employee emp = await GetEmployeeAsync(employeeId, databaseName);
                    employeeprofile epData = await GetEmployeeProfileDataAsync(employeeId, databaseName);

                    string dakarEmployeeCode = string.Empty;

                    if (epData != null && !string.IsNullOrWhiteSpace(epData.employeeCode))
                        dakarEmployeeCode = epData.employeeCode;
                    else
                        dakarEmployeeCode = emp?.email ?? string.Empty;

                    await SavePunchToCommonDbInternalAsync(
                        baseComp.companyId,
                        employeeId,
                        dakarEmployeeCode,
                        punchObj.employeePunchHistoryID,
                        punchType,
                        companyObj?.peNumber ?? string.Empty,
                        locationId,
                        locationCode,
                        baseTime,
                        saveUtcNow,
                        actualCompanyTime,
                        latitude,
                        longitude);
                }
                catch
                {
                    // preserve old behavior
                }
            }

            return punchObj;
        }

        public async Task<punchlocation> SaveExactPunchCoordinatesAsync(
            int punchHistoryId,
            double latitude,
            double longitude,
            string employeeEmail,
            string databaseName,
            int punchAttributeValueId,
            string punchAttributeValue)
        {
            var now = DateTime.Now;

            var newPunchCoordinates = new punchlocation
            {
                punchHistoryID = punchHistoryId,
                Latitude = latitude,
                Longitude = longitude,
                punchAttributeValueID = punchAttributeValueId,
                punchAttributeValue = punchAttributeValue,
                createdBy = employeeEmail,
                createdOn = now,
                updatedBy = employeeEmail,
                updatedOn = now
            };

            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            object id = context.Insert(newPunchCoordinates);
            newPunchCoordinates.punchLocationID = Convert.ToInt32(id);

            return await Task.FromResult(newPunchCoordinates);
        }

        public async Task<company> GetCompanyFromCompIdAsync(int companyId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("companies")
                .Where("companyID = @0", companyId);

            return await Task.FromResult(context.Fetch<company>(ppSql).FirstOrDefault());
        }

        private async Task<employee> GetEmployeeAsync(int employeeId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("employees")
                .Where("employeeID = @0 and isDeleted = 0", employeeId);

            return await Task.FromResult(context.Fetch<employee>(ppSql).FirstOrDefault());
        }

        private async Task<employeeprofile> GetEmployeeProfileDataAsync(int employeeId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("employeeprofile ep")
                .InnerJoin("employees e").On("ep.employeeID = e.employeeID")
                .Where("e.isDeleted != 1 and e.employeeID = @0", employeeId);

            return await Task.FromResult(context.Fetch<employeeprofile>(ppSql).FirstOrDefault());
        }

        private async Task<timezone> GetCompanyTimeZoneDetailsFromCompanyDbAsync(int timeZoneId, string databaseName)
        {
            var context = DataContextHelper.GetCompanyDataContext(databaseName);

            var ppSql = Sql.Builder
                .Select("*")
                .From("timezones")
                .Where("timezoneID = @0", timeZoneId);

            return await Task.FromResult(context.Fetch<timezone>(ppSql).FirstOrDefault());
        }

        private async Task<punch> SavePunchToCommonDbInternalAsync(
            int companyId,
            int employeeId,
            string dakarEmployeeCode,
            int punchHistoryId,
            int punchType,
            string peNumber,
            int locationId,
            string locationCode,
            DateTime baseTime,
            DateTime saveUtcNow,
            DateTime actualCompanyTime,
            double latitude,
            double longitude)
        {
            var punchObj = new punch
            {
                companyID = companyId,
                employeeID = employeeId,
                dakarEmployeeCode = dakarEmployeeCode,
                punchHistoryID = punchHistoryId,
                Latitude = latitude,
                Longitude = longitude,
                LocationCode = locationCode,
                peNumber = peNumber,
                punchTime = baseTime,
                punchTimeUTC = saveUtcNow,
                punchTimeCountry = actualCompanyTime,
                punchType = punchType,
                isSyncDakar = false,
                isDeleted = false,
                createdBy = dakarEmployeeCode,
                updatedBy = dakarEmployeeCode,
                createdOn = DateTime.Now,
                updatedOn = DateTime.Now
            };

            var context = DataContextHelper.GetCompanyDataContext("dakarpunches");

            object id = context.Insert(punchObj);
            punchObj.punchID = Convert.ToInt32(id);

            return await Task.FromResult(punchObj);
        }
    }
}