using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.ActivityServices
{
    public class ActivityService : IActivityService
    {
        public Task<List<Activity>> GetAllActivitiesAsync(string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading activities from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("Id, name as Name, color as Color, description as Description")
                    .From("activities")
                    .Where("isActive = 1")
                    .OrderBy("Id desc");

                var activities = db.Fetch<Activity>(sql).ToList();

                AppLogger.Info(
                    message: "Activities loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, ActivitiesCount={activities.Count}");

                return Task.FromResult(activities);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load activities from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> AddActivityAsync(string databaseName, string userId, string name, string description, string color)
        {
            try
            {
                AppLogger.Debug(
                    message: "Creating activity in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, ActivityName={name}");

                DateTime now = DateTime.Now;

                activities activity = new activities
                {
                    name = name,
                    description = description,
                    color = color,
                    createdOn = now,
                    createdBy = userId,
                    isActive = true
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(activity);

                AppLogger.Info(
                    message: "Activity created successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, ActivityName={name}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to create activity in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, ActivityName={name}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> UpdateActivityAsync(string databaseName, string userId, int activityId, string name, string description, string color)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating activity in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, ActivityId={activityId}, ActivityName={name}");

                DateTime now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                int rowsAffected = db.Execute(
                    "UPDATE activities SET name = @0, description = @1, color = @2, updatedOn = @3, updatedBy = @4, isActive = @5 WHERE Id = @6",
                    name,
                    description,
                    color,
                    now,
                    userId,
                    true,
                    activityId);

                bool isUpdated = rowsAffected > 0;

                if (!isUpdated)
                {
                    AppLogger.Warn(
                        message: "Update activity failed because activity was not found",
                        action: "DatabaseWrite",
                        result: "NotFound",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, ActivityId={activityId}");

                    return Task.FromResult(false);
                }

                AppLogger.Info(
                    message: "Activity updated successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, ActivityId={activityId}, ActivityName={name}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update activity in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, ActivityId={activityId}, ActivityName={name}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> DeleteActivityAsync(string databaseName, int activityId)
        {
            try
            {
                AppLogger.Debug(
                    message: "Deleting activity from database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, ActivityId={activityId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                int rowsAffected = db.Execute(
                    "UPDATE activities SET IsActive = @0 WHERE Id = @1",
                    false,
                    activityId);

                bool isDeleted = rowsAffected > 0;

                if (!isDeleted)
                {
                    AppLogger.Warn(
                        message: "Delete activity failed because activity was not found",
                        action: "DatabaseWrite",
                        result: "NotFound",
                        updatedBy: string.Empty,
                        description: $"DatabaseName={databaseName}, ActivityId={activityId}");

                    return Task.FromResult(false);
                }

                AppLogger.Info(
                    message: "Activity deleted successfully from database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, ActivityId={activityId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to delete activity from database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, ActivityId={activityId}",
                    exception: ex);

                throw;
            }
        }
    }
}