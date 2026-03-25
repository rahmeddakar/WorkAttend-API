using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.Common.Helper
{
    public class CompanyRegistrationHelper
    {
        private static readonly List<string> TemplateTables = new List<string>
        {
            "actions","activities","adminresettoken","adminroles","admins","appexceptions","companies","companyadmin",
            "companyconfigs","companydepartment","companylocations","countries","currencytypes","departments","employeedatalogs","employeeactivityhistory",
            "employeedepartment","employeedepartmenthistory","employeejobs","employeelocation","employeemobileapp","employeeprofile","employeeprofileattributes",
            "employeepunchhistory","employeequestionaireanswers","employeequestioniare","employees","employeeschedule","jobs","locations","manualpunches",
            "permissions","profileattributes","punchattributes","punchattributevalues","punchlocation","questionaire",
            "questionairequestions","questionaireresults","questionairescales","questionnairescaletype","questions",
            "questiontypedata","questiontypes","roles","timezones","projects","clients","projectclient","checkin_reset_rules"
        };

        public Task<List<country>> GetCountriesAsync()
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("countries")
                .OrderBy("country_name asc");

            return Task.FromResult(db.Fetch<country>(sql).ToList());
        }

        public Task<List<currencytype>> GetCurrenciesAsync()
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("currencytypes")
                .Where("IsDelete != 1 and IsActive = 1")
                .OrderBy("CurrencyName asc");

            return Task.FromResult(db.Fetch<currencytype>(sql).ToList());
        }

        public Task<List<industry>> GetIndustriesAsync()
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("industries")
                .OrderBy("industryName asc");

            return Task.FromResult(db.Fetch<industry>(sql).ToList());
        }

        public Task<List<company>> CheckCompanyExistAsync(string peNumber)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("companies")
                .Where("peNumber = @0 and isDeleted != 1", peNumber);

            return Task.FromResult(db.Fetch<company>(sql).ToList());
        }

        public Task<Companyconfigurations?> GetCompanyConfigurationsAsync(string companyURL)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("companyconfigurations")
                .Where("companyURL = @0", companyURL);

            return Task.FromResult(db.Fetch<Companyconfigurations>(sql).FirstOrDefault());
        }

        public Task<databasemanager?> GetAvailableDbAsync()
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("databasemanager")
                .Where("isAssigned = 0")
                .OrderBy("databasemanagerID asc")
                .Append("LIMIT 1");

            return Task.FromResult(db.Fetch<databasemanager>(sql).FirstOrDefault());
        }

        public Task<bool> AssignDatabaseAsync(int databasemanagerID, string companyName)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var now = DateTime.Now;

            int rowsAffected = db.Execute(
                "UPDATE databasemanager SET isAssigned = 1, updatedOn = @0, updatedBy = @1 WHERE databasemanagerID = @2",
                now,
                companyName,
                databasemanagerID);

            return Task.FromResult(rowsAffected > 0);
        }

        public Task<bool> CreateCompanyDataBaseAsync(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                return Task.FromResult(false);

            if (!Regex.IsMatch(databaseName, @"^[A-Za-z0-9._]+$"))
                return Task.FromResult(false);

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            db.Execute($"CREATE DATABASE IF NOT EXISTS `{databaseName}`;");
            return Task.FromResult(true);
        }

        public Task<bool> CopyAllTablesAsync(string newDatabaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(newDatabaseName);
            using var db = repository.GetDatabase();

            foreach (var table in TemplateTables)
            {
                db.Execute($"CREATE TABLE `{table}` LIKE `workattend_dev`.`{table}`;");
                db.Execute($"INSERT INTO `{table}` SELECT * FROM `workattend_dev`.`{table}`;");
            }

            return Task.FromResult(true);
        }

        public Task<int> GetTimeZoneIdFromNameAsync(string timeZoneName)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("timezones")
                .Where("name = @0", timeZoneName);

            var tz = db.Fetch<timezone>(sql).FirstOrDefault();

            if (tz != null && tz.timezoneID > 0)
                return Task.FromResult(tz.timezoneID);

            return Task.FromResult(1);
        }

        public Task<string> GetCountryNameByIdAsync(int countryId)
        {
            if (countryId <= 0)
                return Task.FromResult(string.Empty);

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("countries")
                .Where("id = @0", countryId);

            var countryData = db.Fetch<country>(sql).FirstOrDefault();
            return Task.FromResult(countryData?.country_name ?? string.Empty);
        }

        public Task<bool> GetCompanyconfigFromAppkeyAsync(string mobileAppKey)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("companyconfigurations")
                .Where("mobileApplicationKey = @0", mobileAppKey);

            var companyConfig = db.Fetch<Companyconfigurations>(sql).FirstOrDefault();
            return Task.FromResult(companyConfig != null && companyConfig.companyConfigID > 0);
        }

        public async Task<string> GenerateUniqueMobileApplicationKeyAsync(string companyName)
        {
            string appKey = GenerateMobileApplicationKey(companyName);

            while (await GetCompanyconfigFromAppkeyAsync(appKey))
            {
                appKey = GenerateMobileApplicationKey(companyName);
            }

            return appKey;
        }

        public Task<subscriptionpackage?> GetSubscriptionByNameAsync(string subscriptionName)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("subscriptionpackages")
                .Where("name = @0 and isDeleted != 1", subscriptionName);

            return Task.FromResult(db.Fetch<subscriptionpackage>(sql).FirstOrDefault());
        }

        public Task<List<subscriptionpackagefeatures>> GetSubscriptionPackageFeaturesAsync(int subscriptionPackageID)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("subscriptionpackagefeatures")
                .Where("isDeleted = 0 and isSystem = 0 and subscriptionPackageID = @0", subscriptionPackageID);

            return Task.FromResult(db.Fetch<subscriptionpackagefeatures>(sql).ToList());
        }

        public Task<companysubscriptionpackage> CreateCompanySubscriptionAsync(int subscriptionID, int companyID)
        {
            DateTime now = DateTime.Now;
            DateTime endDate = now.AddDays(14);

            companysubscriptionpackage newSubscription = new companysubscriptionpackage
            {
                companyID = companyID,
                SubscriptionPackageID = subscriptionID,
                packageStartDate = now,
                packageEndDate = endDate,
                IsActive = true,
                CreatedBy = companyID,
                CreatedOn = now,
                UpdatedOn = now,
                UpdatedBy = companyID
            };

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var id = db.Insert(newSubscription);
            newSubscription.companySubscriptionPackageID = int.Parse(id.ToString()!);

            return Task.FromResult(newSubscription);
        }

        public async Task<bool> AddCompanyFeaturesAsync(int subscriptionPackageID, int companyId)
        {
            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var defaultFeaturesSql = Sql.Builder
                .Select("*")
                .From("subscriptionfeatures")
                .Where("IsDefault = 1 and IsActive = 1");

            var defaultFeatures = db.Fetch<subscriptionfeature>(defaultFeaturesSql).ToList();
            var packageFeatures = await GetSubscriptionPackageFeaturesAsync(subscriptionPackageID);

            foreach (var feature in defaultFeatures)
            {
                var packageFeature = packageFeatures.FirstOrDefault(x => x.SubscriptionFeatureID == feature.SubscriptionFeatureID);

                companysubscriptionpackagefeatures companyFeature = new companysubscriptionpackagefeatures
                {
                    companyId = companyId,
                    subscriptionpackageId = subscriptionPackageID,
                    subscriptionpackagefeatureId = feature.SubscriptionFeatureID,
                    FeatureValue = packageFeature?.FeatureValue ?? 0,
                    isActive = packageFeature?.IsActive ?? false,
                    createdBy = "system",
                    createdOn = DateTime.Now,
                    updatedBy = "system",
                    updatedOn = DateTime.Now,
                    IsDelete = false
                };

                db.Insert(companyFeature);
            }

            return true;
        }

        public Task<bool> GetActivePackageAsync(int companyID)
        {
            DateTime now = DateTime.Now;

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("*")
                .From("companysubscriptionpackages")
                .Where("isActive = 1 and packageEndDate >= @0 and companyID = @1", now, companyID);

            var package = db.Fetch<companysubscriptionpackage>(sql).FirstOrDefault();

            return Task.FromResult(package != null && package.companySubscriptionPackageID > 0);
        }

        private string GenerateMobileApplicationKey(string companyName)
        {
            companyName = (companyName ?? string.Empty).Replace(" ", "");

            string encryptedCompanyName = EncryptionHelper.Encrypt(companyName);
            string randomSeed = StringHelper.GetRandomAlphanumericString(8);

            var normalizedChars = encryptedCompanyName
                .Select(ch => (char.IsLetterOrDigit(ch) ? ch : 'w'))
                .ToArray();

            string seed2 = new string(normalizedChars);
            if (seed2.Length < 4)
                seed2 = seed2.PadRight(4, 'w');

            seed2 = seed2.Substring(0, 4);

            return (randomSeed + seed2).ToLowerInvariant();
        }
    }
}