using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.GeoFenceServices
{
    public interface IGeoFenceService
    {
        Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName);
        Task<int> GetDatabaseLocationCountAsync(string databaseName);
        Task<bool> CreateGeoFenceAsync(string userId, int companyId, Location model, string databaseName);
        Task<bool> IsLocationAssignedToEmployeeAsync(int locationId, string databaseName);
        Task<bool> SoftDeleteLocationAsync(int locationId, string userId, string databaseName);
        Task<LocationModel?> GetLocationAsync(int locationId, string databaseName);
        Task<bool> UpdateLocationAsync(string userId, LocationModel location, string databaseName);
    }
}