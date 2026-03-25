using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using WorkAttend.API.Gateway.DAL.Repositories;

namespace WorkAttend.API.Gateway.DAL.Common.Helper
{
    public static class DataContextHelper
    {
        private static readonly IConfigurationRoot _config;

        static DataContextHelper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Dal.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            if (!File.Exists(Path.Combine(AppContext.BaseDirectory, "appsettings.json")) &&
                !File.Exists(Path.Combine(AppContext.BaseDirectory, "appsettings.Dal.json")))
            {
                builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile("appsettings.Dal.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables();
            }

            _config = builder.Build();
        }

        // 1) Full connection string from appsettings.json
        public static PunchMobileRepository GetWorkAttendBaseContext(bool enableAutoSelect = true)
        {
            var repository = new PunchMobileRepository(_config, "workAttendBaseString");
            repository.EnableAutoSelect = enableAutoSelect;
            return repository;
        }

        // 2) Full connection string from appsettings.json
        public static PunchMobileRepository GetPunchMobileContext(bool enableAutoSelect = true)
        {
            var repository = new PunchMobileRepository(_config, "PunchMobileConnectionString");
            repository.EnableAutoSelect = enableAutoSelect;
            return repository;
        }

        // 3) Dynamic database name using CompanyConnection + base/appsettings values
        public static PunchMobileRepository GetCompanyDataContext(
            string databaseName,
            string? serverName = null,
            int? port = null,
            string? userName = null,
            string? password = null,
            bool enableAutoSelect = true)
        {
            var repository = new PunchMobileRepository(
                _config,
                "CompanyConnection",
                databaseName,
                serverName,
                port,
                userName,
                password);

            repository.EnableAutoSelect = enableAutoSelect;
            return repository;
        }

        // Optional generic helper
        public static PunchMobileRepository GetNamedContext(string connectionStringName, bool enableAutoSelect = true)
        {
            var repository = new PunchMobileRepository(_config, connectionStringName);
            repository.EnableAutoSelect = enableAutoSelect;
            return repository;
        }
    }
}