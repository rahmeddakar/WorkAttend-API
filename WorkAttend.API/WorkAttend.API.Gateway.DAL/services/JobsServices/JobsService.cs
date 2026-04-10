using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.JobsServices
{
    public class JobsService : IJobsService
    {
        public Task<List<Jobs>> GetAllJobsAsync(string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading jobs from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("jobs")
                    .Where("isActive = 1")
                    .OrderBy("Id desc");

                var jobs = db.Fetch<Jobs>(sql).ToList();

                AppLogger.Info(
                    message: "Jobs loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Count={jobs.Count}");

                return Task.FromResult(jobs);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load jobs from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> AddJobAsync(string databaseName, string userId, string jobName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Creating job in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, JobName={jobName}");

                DateTime now = DateTime.Now;

                jobs newJob = new jobs
                {
                    Job = jobName,
                    createdBy = userId,
                    createdOn = now,
                    updatedBy = userId,
                    updatedOn = now,
                    isActive = true
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newJob);

                AppLogger.Info(
                    message: "Job created successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, JobName={jobName}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to create job in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, JobName={jobName}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> UpdateJobAsync(string databaseName, string userId, int jobId, string jobName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating job in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, JobId={jobId}, JobName={jobName}");

                DateTime now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                int rowsAffected = db.Execute(
                    "UPDATE jobs SET Job = @0, updatedOn = @1, updatedBy = @2, isActive = @3 WHERE Id = @4",
                    jobName,
                    now,
                    userId,
                    true,
                    jobId);

                bool isUpdated = rowsAffected > 0;

                AppLogger.Info(
                    message: isUpdated ? "Job updated successfully in database" : "Job update found no matching row",
                    action: "DatabaseWrite",
                    result: isUpdated ? "Success" : "NotFound",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, JobId={jobId}, JobName={jobName}");

                return Task.FromResult(isUpdated);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update job in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, JobId={jobId}, JobName={jobName}",
                    exception: ex);

                throw;
            }
        }
    }
}