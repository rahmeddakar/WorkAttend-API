using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.JobsServices
{
    public interface IJobsService
    {
        Task<List<Jobs>> GetAllJobsAsync(string databaseName);
        Task<bool> AddJobAsync(string databaseName, string userId, string jobName);
        Task<bool> UpdateJobAsync(string databaseName, string userId, int jobId, string jobName);
    }
}