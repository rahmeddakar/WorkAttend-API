using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.GeoFenceServices
{
    public class GeoFenceService : IGeoFenceService
    {
        public Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading geo-fences from database",
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

                var data = db.Fetch<Location>(sql).ToList();

                AppLogger.Info(
                    message: "Geo-fences loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Count={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load geo-fences from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<int> GetDatabaseLocationCountAsync(string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading geo-fence count from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("count(*)")
                    .From("locations l")
                    .Where("l.isDeleted != 1");

                int count = db.Fetch<int>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: "Geo-fence count loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Count={count}");

                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load geo-fence count from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> CreateGeoFenceAsync(string userId, int companyId, Location model, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Creating geo-fence in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, LocationName={model?.LocationName}, LocationCode={model?.LocationCode}");

                DateTime now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                using var tx = db.GetTransaction();

                LocationModel newLocation = new LocationModel
                {
                    latitudeP1 = model.LatitudeP1,
                    latitudeP2 = model.LatitudeP2,
                    latitudeP3 = model.LatitudeP3,
                    latitudeP4 = model.LatitudeP4,
                    longitudeP1 = model.LongitudeP1,
                    longitudeP2 = model.LongitudeP2,
                    longitudeP3 = model.LongitudeP3,
                    longitudeP4 = model.LongitudeP4,
                    locationName = model.LocationName,
                    locationCode = model.LocationCode,
                    createdBy = userId,
                    createdOn = now,
                    updatedBy = userId,
                    updatedOn = now,
                    IsDeleted = false
                };

                object locationIdObj = db.Insert(newLocation);
                int locationId = int.Parse(locationIdObj.ToString()!);

                companylocation newCompanyLocation = new companylocation
                {
                    locationID = locationId,
                    companyID = companyId,
                    createdBy = userId,
                    createdOn = now,
                    updatedBy = userId,
                    updatedOn = now
                };

                db.Insert(newCompanyLocation);

                tx.Complete();

                AppLogger.Info(
                    message: "Geo-fence created successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, LocationId={locationId}, LocationName={model?.LocationName}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to create geo-fence in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, LocationName={model?.LocationName}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> IsLocationAssignedToEmployeeAsync(int locationId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Checking geo-fence employee assignment from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, LocationId={locationId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("COUNT(1)")
                    .From("employeelocation")
                    .Where("locationID = @0", locationId)
                    .Where("isDeleted = @0", false);

                int count = db.ExecuteScalar<int>(sql);

                bool isAssigned = count > 0;

                AppLogger.Info(
                    message: "Geo-fence employee assignment check completed successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, LocationId={locationId}, IsAssigned={isAssigned}");

                return Task.FromResult(isAssigned);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to check geo-fence employee assignment from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, LocationId={locationId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> SoftDeleteLocationAsync(int locationId, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Soft deleting geo-fence in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, LocationId={locationId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var now = DateTime.Now;

                int rowsAffected = db.Update<LocationModel>(
                    "SET isDeleted = @0, updatedOn = @1, updatedBy = @2 WHERE locationID = @3",
                    true,
                    now,
                    userId,
                    locationId);

                bool isDeleted = rowsAffected > 0;

                AppLogger.Info(
                    message: isDeleted ? "Geo-fence soft deleted successfully in database" : "Geo-fence soft delete found no matching row",
                    action: "DatabaseWrite",
                    result: isDeleted ? "Success" : "NotFound",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, LocationId={locationId}");

                return Task.FromResult(isDeleted);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to soft delete geo-fence in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, LocationId={locationId}",
                    exception: ex);

                throw;
            }
        }

        public Task<LocationModel?> GetLocationAsync(int locationId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading geo-fence from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, LocationId={locationId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("locations")
                    .Where("isDeleted != 1 and locationID = @0", locationId);

                var location = db.Fetch<LocationModel>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: location != null ? "Geo-fence loaded successfully from database" : "Geo-fence not found in database",
                    action: "DatabaseRead",
                    result: location != null ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, LocationId={locationId}");

                return Task.FromResult(location);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load geo-fence from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, LocationId={locationId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> UpdateLocationAsync(string userId, LocationModel location, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating geo-fence in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, LocationId={location?.locationID}, LocationName={location?.locationName}");

                DateTime now = DateTime.Now;

                location.createdBy = userId;
                location.createdOn = now;
                location.updatedBy = userId;
                location.updatedOn = now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Update(location);

                AppLogger.Info(
                    message: "Geo-fence updated successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, LocationId={location?.locationID}, LocationName={location?.locationName}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update geo-fence in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, LocationId={location?.locationID}, LocationName={location?.locationName}",
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