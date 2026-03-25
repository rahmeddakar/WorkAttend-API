using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.ActivityServices
{
    public interface IActivityService
    {
        Task<List<Activity>> GetAllActivitiesAsync(string databaseName);
        Task<bool> AddActivityAsync(string databaseName, string userId, string name, string description, string color);
        Task<bool> UpdateActivityAsync(string databaseName, string userId, int activityId, string name, string description, string color);
        Task<bool> DeleteActivityAsync(string databaseName, int activityId);
        Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage);
    }
}