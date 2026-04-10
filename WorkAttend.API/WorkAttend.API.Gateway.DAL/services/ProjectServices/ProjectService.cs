using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.ProjectServices
{
    public class ProjectService : IProjectService
    {
        public Task<List<Project>> GetAllProjectsAsync(string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading projects from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("p.Id, p.name as Name, p.code as Code, p.locationID, p.description as Description, COALESCE(l.locationName, 'Unknown') as LocationName")
                    .From("projects p")
                    .LeftJoin("locations l").On("l.locationID = p.locationID")
                    .Where("p.isActive = 1")
                    .OrderBy("p.Id desc");

                var data = db.Fetch<Project>(sql).ToList();

                AppLogger.Info(
                    message: "Projects loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Count={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load projects from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading project locations from database",
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
                    message: "Project locations loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Count={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load project locations from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<projects> AddProjectAsync(string databaseName, string userId, string projectName, string projectCode, string description, int locationId)
        {
            try
            {
                DateTime now = DateTime.Now;

                projects newProject = new projects
                {
                    locationID = locationId,
                    name = projectName,
                    code = projectCode,
                    description = description,
                    createdOn = now,
                    createdBy = userId,
                    updatedOn = now,
                    updatedBy = userId,
                    isActive = true
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                object id = db.Insert(newProject);
                newProject.Id = int.Parse(id.ToString()!);

                return Task.FromResult(newProject);
            }
            catch
            {
                throw;
            }
        }

        public Task<bool> UpdateProjectAsync(string databaseName, string userId, int projectId, string projectName, string projectCode, string description, int locationId)
        {
            try
            {
                DateTime now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                int rowsAffected = db.Execute(
                    "UPDATE projects SET locationID = @0, name = @1, code = @2, description = @3, updatedOn = @4, updatedBy = @5, isActive = @6 WHERE Id = @7",
                    locationId,
                    projectName,
                    projectCode,
                    description,
                    now,
                    userId,
                    true,
                    projectId);

                return Task.FromResult(rowsAffected > 0);
            }
            catch
            {
                throw;
            }
        }

        public Task<Project?> GetProjectByIdAsync(int projectId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("p.Id, p.name as Name, p.code as Code, p.locationID, p.description as Description, COALESCE(l.locationName, 'Unknown') as LocationName")
                .From("projects p")
                .LeftJoin("locations l").On("l.locationID = p.locationID")
                .Where("p.Id = @0", projectId);

            var project = db.Fetch<Project>(sql).FirstOrDefault();
            return Task.FromResult(project);
        }

        public Task<clients> AddClientAsync(string databaseName, string userId, int companyId, string clientName, string clientCode)
        {
            DateTime now = DateTime.Now;

            clients newClient = new clients
            {
                companyID = companyId,
                name = clientName,
                code = clientCode,
                createdOn = now,
                createdBy = userId,
                updatedOn = now,
                updatedBy = userId
            };

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            object id = db.Insert(newClient);
            newClient.clientID = int.Parse(id.ToString()!);

            return Task.FromResult(newClient);
        }

        public Task<projectClient> AddProjectClientAsync(string databaseName, string userId, int projectId, int clientId)
        {
            DateTime now = DateTime.Now;

            projectClient newProjectClient = new projectClient
            {
                projectID = projectId,
                clientID = clientId,
                createdOn = now,
                createdBy = userId,
                updatedOn = now,
                updatedBy = userId
            };

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            object id = db.Insert(newProjectClient);
            newProjectClient.projectClientID = int.Parse(id.ToString()!);

            return Task.FromResult(newProjectClient);
        }

        public Task<bool> DeleteProjectAsync(string databaseName, int projectId)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            int rowsAffected = db.Execute("UPDATE projects SET IsActive = @0 WHERE Id = @1", false, projectId);
            return Task.FromResult(rowsAffected > 0);
        }
    }
}