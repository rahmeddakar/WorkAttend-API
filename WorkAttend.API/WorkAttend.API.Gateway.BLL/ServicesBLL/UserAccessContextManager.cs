using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.services.AdminsServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class UserAccessContextManager : IUserAccessContextManager
    {
        private readonly IMemoryCache _cache;
        private readonly IAdminsService _adminsService;

        public UserAccessContextManager(IMemoryCache cache, IAdminsService adminsService)
        {
            _cache = cache;
            _adminsService = adminsService;
        }

        public async Task<UserAccessContext?> GetAsync(CurrentUserContext currentUserContext, bool forceRefresh = false)
        {
            AppLogger.Debug(
                    message: "Resolving user access context",
                    action: "AccessContext",
                    result: "Started",
                    updatedBy: currentUserContext.UserId,
                    description: $"DatabaseName={currentUserContext.DatabaseName}");

            try
            {
                string cacheKey = GetCacheKey(currentUserContext.DatabaseName, currentUserContext.UserId);

                if (forceRefresh)
                {
                    _cache.Remove(cacheKey);
                }

                if (_cache.TryGetValue(cacheKey, out UserAccessContext? cached) && cached != null)
                {
                    AppLogger.Debug(
                        message: "User access context cache hit",
                        action: "AccessContext",
                        result: "CacheHit",
                        updatedBy: currentUserContext.UserId,
                        description: $"CacheKey={cacheKey}");

                    return cached;
                }

                AppLogger.Debug(
                        message: "User access context cache miss",
                        action: "AccessContext",
                        result: "CacheMiss",
                        updatedBy: currentUserContext.UserId,
                        description: $"CacheKey={cacheKey}");

                var dbContext = await _adminsService.GetUserAccessContextAsync(
                    currentUserContext.UserId,
                    currentUserContext.DatabaseName);

                if (dbContext == null)
                    return null;

                dbContext.UserId = currentUserContext.UserId;
                dbContext.Email = currentUserContext.Email;
                dbContext.DatabaseName = currentUserContext.DatabaseName;
                dbContext.CompanyURL = currentUserContext.CompanyURL;
                dbContext.DepartmentId = 0;

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                };

                _cache.Set(cacheKey, dbContext, cacheOptions);
                
                AppLogger.Debug(
                        message: "User access context cached successfully",
                        action: "AccessContext",
                        result: "Cached",
                        updatedBy: currentUserContext.UserId,
                        description: $"CacheKey={cacheKey}");

                return dbContext;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to resolve user access context",
                    action: "AccessContext",
                    result: "Failed",
                    updatedBy: currentUserContext.UserId,
                    description: ex.Message,
                    exception: ex);

                return null;
            }
        }

        public void Remove(string databaseName, string userId)
        {
            _cache.Remove(GetCacheKey(databaseName, userId));
        }

        private static string GetCacheKey(string databaseName, string userId)
        {
            return $"useraccess:{databaseName}:{userId}";
        }        
        
    }
}