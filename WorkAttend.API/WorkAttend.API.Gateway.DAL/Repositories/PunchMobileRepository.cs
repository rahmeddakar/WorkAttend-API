using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace WorkAttend.API.Gateway.DAL.Repositories
{
    public class PunchMobileRepository
    {
        private readonly string _connectionString;
        private readonly string _providerName;
        private const string DefaultProviderName = "MySql.Data.MySqlClient";

        public bool EnableAutoSelect { get; set; }

        // 1) Default constructor -> PunchMobileConnectionString
        public PunchMobileRepository(IConfiguration configuration)
            : this(configuration, "PunchMobileConnectionString")
        {
        }

        // 2) Named connection string constructor
        //    Used for workAttendBaseString or PunchMobileConnectionString
        public PunchMobileRepository(IConfiguration configuration, string connectionStringName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _providerName = DefaultProviderName;

            _connectionString = configuration.GetConnectionString(connectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{connectionStringName}' was not found in configuration.");
        }

        // 3) Raw connection string + provider name
        public PunchMobileRepository(IConfiguration configuration, string connectionStringOrName, string providerName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _providerName = string.IsNullOrWhiteSpace(providerName)
                ? DefaultProviderName
                : providerName;

            var maybeConn = configuration.GetConnectionString(connectionStringOrName);
            _connectionString = !string.IsNullOrWhiteSpace(maybeConn)
                ? maybeConn
                : connectionStringOrName ?? throw new InvalidOperationException("Connection string is null or empty.");
        }

        // 4) CompanyConnection constructor
        //    Uses CompanyConnection template if available, otherwise builds from workAttendBaseString/appsettings values.
        public PunchMobileRepository(
            IConfiguration configuration,
            string connectionStringName,
            string databaseName,
            string? serverName = null,
            int? port = null,
            string? userName = null,
            string? password = null)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("databaseName is required.", nameof(databaseName));

            _providerName = DefaultProviderName;

            var template = configuration.GetConnectionString(connectionStringName);
            var baseConnection = configuration.GetConnectionString("workAttendBaseString");

            // Prefer explicit parameters
            // If missing, try AppSettings
            // If still missing, try workAttendBaseString
            var finalServer = !string.IsNullOrWhiteSpace(serverName)
                ? serverName
                : configuration["AppSettings:dbserver"];

            var finalUser = !string.IsNullOrWhiteSpace(userName)
                ? userName
                : configuration["AppSettings:username"];

            var finalPassword = password ?? configuration["AppSettings:password"];

            uint finalPort = (uint)(port.HasValue && port.Value > 0 ? port.Value : 3306);

            if (!string.IsNullOrWhiteSpace(baseConnection))
            {
                var baseBuilder = new MySqlConnectionStringBuilder(baseConnection);

                if (string.IsNullOrWhiteSpace(finalServer))
                    finalServer = baseBuilder.Server;

                if (string.IsNullOrWhiteSpace(finalUser))
                    finalUser = baseBuilder.UserID;

                if (string.IsNullOrWhiteSpace(finalPassword))
                    finalPassword = baseBuilder.Password;

                if ((!port.HasValue || port.Value <= 0) && baseBuilder.Port > 0)
                    finalPort = baseBuilder.Port;
            }

            if (!string.IsNullOrWhiteSpace(template))
            {
                // Replace placeholders if template style is used
                var editable = template
                    .Replace("[serverAddress]", finalServer ?? "localhost")
                    .Replace("[database]", databaseName)
                    .Replace("[username]", finalUser ?? "root")
                    .Replace("[password]", finalPassword ?? string.Empty);

                if (editable.Contains("[port]"))
                    editable = editable.Replace("[port]", finalPort.ToString());

                _connectionString = editable;
            }
            else
            {
                // Fallback: build from values
                var builder = new MySqlConnectionStringBuilder
                {
                    Server = finalServer ?? "localhost",
                    Database = databaseName,
                    UserID = finalUser ?? "root",
                    Password = finalPassword ?? string.Empty,
                    Port = finalPort,
                    SslMode = MySqlSslMode.Disabled,
                    AllowPublicKeyRetrieval = true
                };

                _connectionString = builder.ConnectionString;
            }
        }

        public Database GetDatabase()
        {
            return new Database(_connectionString, _providerName);
        }

        public IEnumerable<T> Fetch<T>(string sql, params object[] args)
        {
            using var db = GetDatabase();
            return db.Fetch<T>(sql, args);
        }

        public IEnumerable<T> Fetch<T>(Sql sql)
        {
            using var db = GetDatabase();
            return db.Fetch<T>(sql);
        }

        public T? FirstOrDefault<T>(string sql, params object[] args)
        {
            using var db = GetDatabase();
            return db.FirstOrDefault<T>(sql, args);
        }

        public T? SingleOrDefault<T>(string sql, params object[] args)
        {
            using var db = GetDatabase();
            return db.SingleOrDefault<T>(sql, args);
        }

        public object Insert(object poco)
        {
            using var db = GetDatabase();
            return db.Insert(poco);
        }

        public int Update(object poco)
        {
            using var db = GetDatabase();
            return db.Update(poco);
        }

        public int Delete(object poco)
        {
            using var db = GetDatabase();
            return db.Delete(poco);
        }

        public int Execute(string sql, params object[] args)
        {
            using var db = GetDatabase();
            return db.Execute(sql, args);
        }
    }
}