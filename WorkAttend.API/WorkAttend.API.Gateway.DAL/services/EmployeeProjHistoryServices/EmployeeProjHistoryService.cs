using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.EmployeeProjHistoryServices
{
    public class EmployeeProjHistoryService : IEmployeeProjHistoryService
    {
        public Task<List<Employees>> GetEmployeesAsync(int companyId, int departmentId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading employee project history employees from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentId={departmentId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                Sql sql;

                if (departmentId != 0)
                {
                    sql = Sql.Builder
                        .Select("e.employeeID, e.email, e.createdBy, d.departmentID, d.departmentName, c.companyId, c.name as companyName, CONCAT(e.email, '-', ep.firstname, ' ', ep.surname) as empDisplayName, ep.firstname as firstName, ep.surname as surName, ep.dob")
                        .From("employees e")
                        .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                        .InnerJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .InnerJoin("companies c").On("c.companyid = e.companyid")
                        .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0 And ed.departmentID = @1", companyId, departmentId);
                }
                else
                {
                    sql = Sql.Builder
                        .Select("e.employeeID, e.email, e.createdBy, d.departmentID, d.departmentName, c.companyId, c.name as companyName, CONCAT(e.email, '-', ep.firstname, ' ', ep.surname) as empDisplayName, ep.firstname as firstName, ep.surname as surName, ep.dob")
                        .From("employees e")
                        .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                        .LeftJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                        .LeftJoin("departments d").On("d.departmentID = ed.departmentID")
                        .InnerJoin("companies c").On("c.companyid = e.companyid")
                        .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0", companyId);
                }

                var employees = db.Fetch<Employees>(sql).ToList();

                AppLogger.Info(
                    message: "Employee project history employees loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentId={departmentId}, Count={employees.Count}");

                return Task.FromResult(employees);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load employee project history employees from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentId={departmentId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<Location>> GetLocationsAsync(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading employee project history locations from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("l.locationid as Id, l.locationName as LocationName, l.locationCode as LocationCode, l.latitudeP1 as LatitudeP1, l.longitudeP1 as LongitudeP1, l.latitudeP2 as LatitudeP2, l.longitudeP2 as LongitudeP2, l.latitudeP3 as LatitudeP3, l.longitudeP3 as LongitudeP3, l.latitudeP4 as LatitudeP4, l.longitudeP4 as LongitudeP4")
                    .From("companylocations cl")
                    .InnerJoin("locations l").On("l.locationID = cl.locationID")
                    .Where("l.isDeleted != 1 and cl.companyID = @0", companyId)
                    .OrderBy("l.locationid desc");

                var locations = db.Fetch<Location>(sql).ToList();

                AppLogger.Info(
                    message: "Employee project history locations loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Count={locations.Count}");

                return Task.FromResult(locations);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load employee project history locations from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
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