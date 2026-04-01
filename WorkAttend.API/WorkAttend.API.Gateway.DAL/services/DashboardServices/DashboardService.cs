using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.DashboardServices
{
    public class DashboardService : IDashboardService
    {
        public Task<int> GetEmployeesCountAsync(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading dashboard employee count from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("count(*)")
                    .From("employees e")
                    .Where("e.isDeleted != 1 and e.companyID = @0", companyId);

                int count = db.Fetch<int>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: "Dashboard employee count loaded successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Count={count}");

                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load dashboard employee count from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<employeepunchhistory>> GetPunchesCountAsync(int companyId, DateTime punchTimeStart, DateTime punchTimeEnd, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading dashboard punch history from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Start={punchTimeStart:O}, End={punchTimeEnd:O}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("ep.*")
                    .From("employeepunchhistory ep")
                    .InnerJoin("employees e").On("e.employeeID = ep.employeeID")
                    .Where("e.isDeleted != 1 and e.companyID = @0 and ep.punchTimeCountry >= @1 and ep.punchTimeCountry <= @2", companyId, punchTimeStart, punchTimeEnd);

                var data = db.Fetch<employeepunchhistory>(sql).ToList();

                AppLogger.Info(
                    message: "Dashboard punch history loaded successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Count={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load dashboard punch history from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Start={punchTimeStart:O}, End={punchTimeEnd:O}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<resultsQuest>> GetResultsAsync(int companyId, DateTime startDate, DateTime endDate, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading dashboard questionnaire results from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Start={startDate:O}, End={endDate:O}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("qr.questionaireResultsID, qr.questionaireScaleID, qr.range, qr.rangeText, CONCAT(er.firstname, ' ', er.surname) as name, e.email")
                    .From("questionaireresults qr")
                    .InnerJoin("employees e").On("e.employeeID = qr.employeeID")
                    .InnerJoin("employeeprofile er").On("er.employeeID = e.employeeID")
                    .Where("e.isDeleted != 1 and e.companyID = @0", companyId)
                    .Where("questionaireResultsID in (SELECT MAX(questionaireResultsID) FROM questionaireresults GROUP BY employeeid order by questionaireResultsID desc)")
                    .Where("qr.createdOn >= @0 and qr.createdOn <= @1", startDate, endDate);

                var data = db.Fetch<resultsQuest>(sql).ToList();

                AppLogger.Info(
                    message: "Dashboard questionnaire results loaded successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Count={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load dashboard questionnaire results from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Start={startDate:O}, End={endDate:O}",
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