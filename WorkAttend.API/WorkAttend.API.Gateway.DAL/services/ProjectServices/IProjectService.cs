using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.ProjectServices
{
    public interface IProjectService
    {
        Task<List<Project>> GetAllProjectsAsync(string databaseName);
        Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName);
        Task<projects> AddProjectAsync(string databaseName, string userId, string projectName, string projectCode, string description, int locationId);
        Task<bool> UpdateProjectAsync(string databaseName, string userId, int projectId, string projectName, string projectCode, string description, int locationId);
        Task<Project?> GetProjectByIdAsync(int projectId, string databaseName);
        Task<clients> AddClientAsync(string databaseName, string userId, int companyId, string clientName, string clientCode);
        Task<projectClient> AddProjectClientAsync(string databaseName, string userId, int projectId, int clientId);
        Task<bool> DeleteProjectAsync(string databaseName, int projectId);
    }
}