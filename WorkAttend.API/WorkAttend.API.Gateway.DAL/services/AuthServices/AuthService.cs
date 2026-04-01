using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.AuthServices
{
    public class AuthService : IAuthService
    {
        public Task<Companyconfigurations?> GetCompanyConfigurationAsync(string companyURL)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading company configuration for auth flow",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={companyURL}");

                var repository = DataContextHelper.GetWorkAttendBaseContext();
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("companyconfigurations")
                    .Where("companyURL = @0", companyURL);

                var companyConfig = db.Fetch<Companyconfigurations>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: companyConfig != null ? "Company configuration loaded successfully" : "Company configuration not found",
                    action: "DatabaseRead",
                    result: companyConfig != null ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={companyURL}");

                return Task.FromResult(companyConfig);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load company configuration for auth flow",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={companyURL}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> GetActivePackageAsync(int companyId)
        {
            try
            {
                AppLogger.Debug(
                    message: "Checking active package for auth flow",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}");

                DateTime now = DateTime.Now;

                var repository = DataContextHelper.GetWorkAttendBaseContext();
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("companysubscriptionpackages")
                    .Where("isActive = 1 and packageEndDate >= @0 and companyID = @1", now, companyId);

                var package = db.Fetch<companysubscriptionpackage>(sql).FirstOrDefault();
                bool isValid = package != null && package.companySubscriptionPackageID > 0;

                AppLogger.Info(
                    message: isValid ? "Active package found for auth flow" : "Active package not found for auth flow",
                    action: "DatabaseRead",
                    result: isValid ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}");

                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to check active package for auth flow",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<workattendadmin?> GetAdminFromEmailAsync(string email, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading admin by email for auth flow",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: email,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("admins")
                    .Where("email = @0 and isDeleted != 1", email);

                var admin = db.Fetch<workattendadmin>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: admin != null ? "Admin loaded by email successfully" : "Admin not found by email",
                    action: "DatabaseRead",
                    result: admin != null ? "Success" : "NotFound",
                    updatedBy: email,
                    description: $"DatabaseName={databaseName}");

                return Task.FromResult(admin);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load admin by email for auth flow",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: email,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<workattendadmin?> GetAdminByIdAsync(int adminId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading admin by id for auth flow",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("admins")
                    .Where("adminID = @0 and isDeleted != 1", adminId);

                var admin = db.Fetch<workattendadmin>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: admin != null ? "Admin loaded by id successfully" : "Admin not found by id",
                    action: "DatabaseRead",
                    result: admin != null ? "Success" : "NotFound",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}");

                return Task.FromResult(admin);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load admin by id for auth flow",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> IsRedundantRequestAsync(int adminId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Checking redundant password reset request",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}");

                var now = DateTime.Now;
                var lastThirtyMinutes = now.AddMinutes(-30);

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("adminresetTokenID")
                    .From("adminresettoken")
                    .Where("adminID = @0 and createdOn >= @1", adminId, lastThirtyMinutes);

                var allTokens = db.Fetch<int>(sql).ToList();
                bool isRedundant = allTokens.Count > 2;

                AppLogger.Info(
                    message: "Redundant password reset request check completed",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}, IsRedundant={isRedundant}");

                return Task.FromResult(isRedundant);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to check redundant password reset request",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public async Task<adminresettoken?> StorePasswordResetTokenAsync(string adminEmail, int adminId, string token, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Storing password reset token",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: adminEmail,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}");

                DateTime now = DateTime.Now;
                DateTime expiryDate = now.AddHours(2);

                var existingTokenIds = await GetAdminTokensAsync(databaseName, adminId);
                if (existingTokenIds.Count > 0)
                {
                    await ExpireAllTokensAsync(databaseName, existingTokenIds, adminId);
                }

                adminresettoken newAdminResetToken = new adminresettoken
                {
                    adminID = adminId,
                    token = token,
                    expiryDate = expiryDate,
                    createdOn = now,
                    createdBy = adminEmail,
                    updatedOn = now,
                    updatedBy = adminEmail,
                    isDeleted = false
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                object id = db.Insert(newAdminResetToken);
                newAdminResetToken.adminresettokenID = int.Parse(id.ToString()!);

                AppLogger.Info(
                    message: "Password reset token stored successfully",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: adminEmail,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, ResetTokenId={newAdminResetToken.adminresettokenID}");

                return newAdminResetToken;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to store password reset token",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: adminEmail,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}",
                    exception: ex);

                throw;
            }
        }

        public Task<adminresettoken?> CheckTokenExistAsync(int adminId, string token, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Validating password reset token",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}");

                DateTime now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("adminresettoken")
                    .Where("adminID = @0 and token = @1 and expiryDate >= @2 and isDeleted != 1", adminId, token, now);

                var resetToken = db.Fetch<adminresettoken>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: resetToken != null ? "Password reset token validated successfully" : "Password reset token was invalid or expired",
                    action: "DatabaseRead",
                    result: resetToken != null ? "Success" : "Failed",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}");

                return Task.FromResult(resetToken);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to validate password reset token",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<int>> GetAdminTokensAsync(string databaseName, int adminId)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading password reset token ids for admin",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("adminresetTokenID")
                    .From("adminresettoken")
                    .Where("adminID = @0 and isDeleted != 1", adminId);

                var tokenIds = db.Fetch<int>(sql).ToList();

                AppLogger.Info(
                    message: "Password reset token ids loaded successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}, Count={tokenIds.Count}");

                return Task.FromResult(tokenIds);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load password reset token ids for admin",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> ExpireAllTokensAsync(string databaseName, List<int> tokenIds, int adminId)
        {
            try
            {
                if (tokenIds == null || tokenIds.Count == 0)
                    return Task.FromResult(true);

                AppLogger.Debug(
                    message: "Expiring password reset tokens",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}, TokenCount={tokenIds.Count}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var now = DateTime.Now;

                db.Update<adminresettoken>(
                    "SET isDeleted = 1, updatedOn = @0, updatedBy = @1 WHERE adminresettokenid in (@2)",
                    now,
                    adminId.ToString(),
                    tokenIds);

                AppLogger.Info(
                    message: "Password reset tokens expired successfully",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}, TokenCount={tokenIds.Count}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to expire password reset tokens",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}, TokenCount={tokenIds?.Count ?? 0}",
                    exception: ex);

                throw;
            }
        }

        public async Task<bool> UpdateAdminPasswordAsync(int adminId, string password, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating admin password",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}");

                string encryptedPassword = EncryptionHelper.Encrypt(password);
                var admin = await GetAdminByIdAsync(adminId, databaseName);

                if (admin == null || admin.adminID <= 0)
                {
                    AppLogger.Warn(
                        message: "Update admin password failed because admin was not found",
                        action: "DatabaseWrite",
                        result: "NotFound",
                        updatedBy: adminId.ToString(),
                        description: $"DatabaseName={databaseName}");

                    return false;
                }

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var now = DateTime.Now;

                db.Update<workattendadmin>(
                    "SET password = @0, updatedOn = @1, updatedBy = @2 WHERE adminID = @3",
                    encryptedPassword,
                    now,
                    admin.email ?? adminId.ToString(),
                    admin.adminID);

                AppLogger.Info(
                    message: "Admin password updated successfully",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}, AdminId={admin.adminID}");

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update admin password",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task InsertException(string source, string message, string originatedAt, string stackTrace, string innerExceptionMessage)
        {
            DateTime now = DateTime.Now;

            appexception newException = new appexception
            {
                source = source,
                message = message,
                originatedAt = originatedAt,
                stacktrace = stackTrace,
                innerexceptionmessage = innerExceptionMessage,
                createdOn = now,
                createdBy = "system",
                updatedOn = now,
                updatedBy = "system"
            };

            var repository = DataContextHelper.GetWorkAttendBaseContext();
            using var db = repository.GetDatabase();

            db.Insert(newException);

            return Task.CompletedTask;
        }
    }
}