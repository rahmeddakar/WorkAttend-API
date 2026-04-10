using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.AdminPanelServices
{
    public class AdminPanelService : IAdminPanelService
    {
        public Task<List<AdminPanelItem>> GetAdminPanelItemsAsync(bool? isDakarConnected, string companyName, int packageId, int limit)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading admin panel items from base database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"IsDakarConnected={isDakarConnected}, CompanyName={companyName}, PackageId={packageId}, Limit={limit}");

                if (limit <= 0)
                    limit = 50;

                if (limit > 500)
                    limit = 500;

                var repository = DataContextHelper.GetWorkAttendBaseContext();
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select(@"
                        c.companyId AS CompanyId,
                        c.name AS Name,
                        c.IsDakarConnected AS IsDakarConnected,
                        cc.databaseName AS DatabaseName,
                        sp.displayName AS DisplayName,
                        csp.packageStartDate AS PackageStartDate,
                        csp.packageEndDate AS PackageEndDate,
                        cc.companyURL AS CompanyURL
                    ")
                    .From("companies c")
                    .InnerJoin("companyconfigurations cc").On("cc.companyid = c.companyId")
                    .InnerJoin("companysubscriptionpackages csp").On("csp.companyid = c.companyId")
                    .InnerJoin("subscriptionpackages sp").On("sp.subscriptionpackageid = csp.subscriptionpackageid")
                    .Where("c.isdeleted = @0 AND csp.IsActive = @1", 0, 1);

                if (isDakarConnected.HasValue)
                {
                    sql.Where("c.IsDakarConnected = @0", isDakarConnected.Value ? 1 : 0);
                }

                if (!string.IsNullOrWhiteSpace(companyName))
                {
                    sql.Where("c.name LIKE @0", "%" + companyName.Trim() + "%");
                }

                if (packageId > 0)
                {
                    sql.Where("sp.subscriptionpackageid = @0", packageId);
                }

                sql.OrderBy("c.companyId");
                sql.Append($" LIMIT {limit}");

                var data = db.Fetch<AdminPanelItem>(sql).ToList();

                AppLogger.Info(
                    message: "Admin panel items loaded successfully from base database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"Count={data.Count}, Limit={limit}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load admin panel items from base database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"IsDakarConnected={isDakarConnected}, CompanyName={companyName}, PackageId={packageId}, Limit={limit}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<subscriptionpackage>> GetSubscriptionPackagesAsync()
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading subscription packages from base database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: string.Empty);

                var repository = DataContextHelper.GetWorkAttendBaseContext();
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("subscriptionpackages")
                    .Where("isDeleted != 1 and isActive = 1")
                    .OrderBy("subscriptionPackageID asc");

                var packages = db.Fetch<subscriptionpackage>(sql).ToList();

                AppLogger.Info(
                    message: "Subscription packages loaded successfully from base database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"Count={packages.Count}");

                return Task.FromResult(packages);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load subscription packages from base database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                throw;
            }
        }

        public Task<AdminPanelItem?> GetCompanyDetailAsync(int companyId)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading admin panel company detail from base database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}");

                var repository = DataContextHelper.GetWorkAttendBaseContext();
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select(@"
                        c.companyId AS CompanyId,
                        c.name AS Name,
                        c.IsDakarConnected AS IsDakarConnected,
                        cc.databaseName AS DatabaseName,
                        sp.displayName AS DisplayName,
                        csp.packageStartDate AS PackageStartDate,
                        csp.packageEndDate AS PackageEndDate,
                        cc.companyURL AS CompanyURL
                    ")
                    .From("companies c")
                    .InnerJoin("companyconfigurations cc").On("cc.companyid = c.companyId")
                    .InnerJoin("companysubscriptionpackages csp").On("csp.companyid = c.companyId")
                    .InnerJoin("subscriptionpackages sp").On("sp.subscriptionpackageid = csp.subscriptionpackageid")
                    .Where("c.isdeleted = @0 AND csp.IsActive = @1 AND c.companyId = @2", 0, 1, companyId)
                    .OrderBy("c.companyId");

                var data = db.Fetch<AdminPanelItem>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: data != null ? "Admin panel company detail loaded successfully" : "Admin panel company detail not found",
                    action: "DatabaseRead",
                    result: data != null ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load admin panel company detail from base database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }


    }
}