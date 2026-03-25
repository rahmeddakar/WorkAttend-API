using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.AdminsServices
{
    public class AdminsService : IAdminsService
    {
        public async Task<string?> GetDatabaseNameByCompanyUrlAsync(string companyUrl)
        {
            try
            {
                AppLogger.Debug(
                    message: "Database lookup by company URL started",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={companyUrl}");

                if (string.IsNullOrWhiteSpace(companyUrl))
                {
                    AppLogger.Warn(
                        message: "Database lookup by company URL failed because company URL was empty",
                        action: "DatabaseRead",
                        result: "InvalidRequest",
                        updatedBy: string.Empty,
                        description: "CompanyURL is null or empty");

                    return null;
                }

                var repository = DataContextHelper.GetWorkAttendBaseContext();
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("companyconfigurations")
                    .Where("LOWER(companyURL) = LOWER(@0)", companyUrl.Trim());

                var config = await db.SingleOrDefaultAsync<Companyconfigurations>(sql);

                if (config == null)
                {
                    AppLogger.Warn(
                        message: "Database lookup by company URL returned no record",
                        action: "DatabaseRead",
                        result: "NotFound",
                        updatedBy: string.Empty,
                        description: $"CompanyURL={companyUrl}");

                    return null;
                }

                AppLogger.Info(
                    message: "Database lookup by company URL succeeded",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={companyUrl}, DatabaseName={config.databaseName}");

                return config.databaseName;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Database lookup by company URL failed",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={companyUrl}",
                    exception: ex);

                throw;
            }
        }

        public async Task<workattendadmin?> ValidateAdminAsync(string databaseName, string email, string encryptedPassword)
        {
            try
            {
                AppLogger.Debug(
                    message: "Validating admin credentials from database",
                    action: "LoginValidation",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Email={email}");

                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    AppLogger.Warn(
                        message: "Admin validation failed because database name was empty",
                        action: "LoginValidation",
                        result: "InvalidRequest",
                        updatedBy: string.Empty,
                        description: $"Email={email}");

                    return null;
                }

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("admins")
                    .Where("email = @0 AND password = @1 AND isDeleted != 1", email, encryptedPassword);

                var admin = await db.SingleOrDefaultAsync<workattendadmin>(sql);

                if (admin == null)
                {
                    AppLogger.Warn(
                        message: "Admin validation failed",
                        action: "LoginValidation",
                        result: "Failed",
                        updatedBy: string.Empty,
                        description: $"DatabaseName={databaseName}, Email={email}");

                    return null;
                }

                AppLogger.Info(
                    message: "Admin validation succeeded",
                    action: "LoginValidation",
                    result: "Success",
                    updatedBy: admin.adminID.ToString(),
                    description: $"DatabaseName={databaseName}, Email={admin.email}");

                return admin;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Admin validation failed with database exception",
                    action: "LoginValidation",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Email={email}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<Roles>> GetRoles(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading roles from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("roles r")
                    .Where("r.companyID = @0", companyId);

                var data = db.Fetch<Roles>(sql).ToList();

                AppLogger.Info(
                    message: "Roles loaded from database successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, RolesCount={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load roles from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<adminMod>> GetAllAdmins(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading admins from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}");

                var admins = new List<adminMod>();

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                if (companyId != 0)
                {
                    var sql = Sql.Builder
                        .Select("a.email as adminEmail, (select name from roles limit 1) as roleName, a.adminID, a.createdOn, a.createdBy")
                        .From("admins a")
                        .InnerJoin("companyadmin ca").On("a.adminID = ca.adminID")
                        .InnerJoin("companies c").On("c.companyid = ca.companyid")
                        .Where("ca.IsSuperAdmin = 1 and a.isdeleted = 0 and c.isdeleted = 0 and ca.companyId = @0", companyId)
                        .OrderBy("a.adminID desc");

                    admins = db.Fetch<adminMod>(sql).ToList();
                }

                AppLogger.Info(
                    message: "Admins loaded from database successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, AdminsCount={admins.Count}");

                return Task.FromResult(admins);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load admins from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<permission>> GetPermissions(string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading permissions from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("permissions p")
                    .Where("p.isDeleted != 1");

                var data = db.Fetch<permission>(sql).ToList();

                AppLogger.Info(
                    message: "Permissions loaded from database successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, PermissionsCount={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load permissions from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<int> GetRolesCount(int companyId, int roleId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading role admin count from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, RoleId={roleId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("count(distinct ar.adminID)")
                    .From("adminroles ar")
                    .InnerJoin("companyAdmin ca").On("ca.AdminID = ar.AdminID")
                    .Where("ca.companyID = @0 and ar.roleID = @1 and ar.isdeleted != 1", companyId, roleId);

                var count = db.Fetch<int>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: "Role admin count loaded successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, RoleId={roleId}, Count={count}");

                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load role admin count from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, RoleId={roleId}",
                    exception: ex);

                throw;
            }
        }

        public Task<Roles?> AddRole(int companyId, string policy, string databaseName, string roleName, string roleDescription, string userId)
        {
            try
            {
                AppLogger.Debug(
                    message: "Inserting role into database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, RoleName={roleName}");

                DateTime now = DateTime.Now;

                Roles newRole = new Roles
                {
                    companyID = companyId,
                    name = roleName,
                    description = roleDescription,
                    policy = policy,
                    createdOn = now,
                    createdBy = userId,
                    updatedOn = now,
                    updatedBy = userId
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newRole);

                AppLogger.Info(
                    message: "Role inserted successfully",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, RoleName={roleName}");

                return Task.FromResult<Roles?>(newRole);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to insert role into database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, RoleName={roleName}",
                    exception: ex);

                throw;
            }
        }

        public Task<subscriptionpackagefeatures?> GetActivePackageData(int companyId, int featureId = 0)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading active package data",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}");

                var repository = DataContextHelper.GetWorkAttendBaseContext();
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("subscriptionpackagefeatures");

                if (featureId > 0)
                {
                    sql.Where("IsActive = 1 AND IsDeleted = 0 and SubscriptionFeatureID = @0", featureId);
                }

                var package = db.Fetch<subscriptionpackagefeatures>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: package != null ? "Active package data loaded successfully" : "Active package data not found",
                    action: "DatabaseRead",
                    result: package != null ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}");

                return Task.FromResult(package);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load active package data",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<SubscriptionPackageFeatureModel>> GetCompanySubscriptionFeatures(int companyId, int featureId = 0)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading company subscription features",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}");

                var repository = DataContextHelper.GetWorkAttendBaseContext();
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("companysubscriptionpackagefeatures cspf");

                if (featureId > 0)
                {
                    sql.Where("cspf.CompanyId = @0 and cspf.subscriptionpackagefeatureid = @1 and cspf.isDelete = 0", companyId, featureId);
                }
                else
                {
                    sql.Where("cspf.CompanyId = @0 and cspf.isDelete = 0", companyId);
                }

                var data = db.Fetch<SubscriptionPackageFeatureModel>(sql).ToList();

                AppLogger.Info(
                    message: "Company subscription features loaded successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}, Count={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load company subscription features",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}",
                    exception: ex);

                throw;
            }
        }

        public Task<int> GetDatabaseAdminCount(string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading database admin count",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("count(*)")
                    .From("admins a")
                    .Where("a.isDeleted != 1");

                var count = db.Fetch<int>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: "Database admin count loaded successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Count={count}");

                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load database admin count",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<workattendadmin> GetAdminFromEmail(string email, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading admin by email",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Email={email}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("admins")
                    .Where("email = @0 and isDeleted != 1", email);

                var admin = db.Fetch<workattendadmin>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: admin != null ? "Admin by email loaded successfully" : "Admin by email not found",
                    action: "DatabaseRead",
                    result: admin != null ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Email={email}");

                return Task.FromResult(admin);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load admin by email",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Email={email}",
                    exception: ex);

                throw;
            }
        }

        public Task<companyadmin?> CheckAdminCompanyExist(int adminId, int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Checking admin company mapping",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("companyadmin")
                    .Where("adminID = @0 and isSuperAdmin = 1 and companyID = @1", adminId, companyId);

                var data = db.Fetch<companyadmin>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: data != null ? "Admin company mapping found" : "Admin company mapping not found",
                    action: "DatabaseRead",
                    result: data != null ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to check admin company mapping",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<companyadmin> AddCompanyAdmin(int adminId, int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Inserting company admin mapping",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}");

                DateTime now = DateTime.Now;

                companyadmin newAdmin = new companyadmin
                {
                    companyId = companyId,
                    adminId = adminId,
                    IsSuperAdmin = true,
                    createdOn = now,
                    createdBy = adminId.ToString(),
                    updatedOn = now,
                    updatedBy = adminId.ToString()
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newAdmin);

                AppLogger.Info(
                    message: "Company admin mapping inserted successfully",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}");

                return Task.FromResult(newAdmin);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to insert company admin mapping",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: adminId.ToString(),
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<workattendadmin> AddAdmin(string adminEmail, string password, string userId, string databaseName, string name)
        {
            try
            {
                AppLogger.Debug(
                    message: "Inserting admin into database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, AdminEmail={adminEmail}");

                string encryptedPass = EncryptionHelper.Encrypt(password);
                DateTime now = DateTime.Now;

                workattendadmin newAdmin = new workattendadmin
                {
                    email = adminEmail,
                    password = encryptedPass,
                    name = name,
                    createdOn = now,
                    createdBy = userId,
                    updatedOn = now,
                    updatedBy = userId
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newAdmin);

                AppLogger.Info(
                    message: "Admin inserted successfully",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, AdminEmail={adminEmail}");

                return Task.FromResult(newAdmin);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to insert admin into database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, AdminEmail={adminEmail}",
                    exception: ex);

                throw;
            }
        }

        public Task<adminroles> AddAdminRole(int adminId, int roleId, string userId, string databaseName, int companyId)
        {
            try
            {
                AppLogger.Debug(
                    message: "Inserting admin role mapping",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, RoleId={roleId}, CompanyId={companyId}");

                DateTime now = DateTime.Now;

                adminroles newAdminRole = new adminroles
                {
                    companyID = companyId,
                    adminID = adminId,
                    roleID = roleId,
                    createdOn = now,
                    createdBy = userId,
                    updatedOn = now,
                    updatedBy = userId
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newAdminRole);

                AppLogger.Info(
                    message: "Admin role mapping inserted successfully",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, RoleId={roleId}, CompanyId={companyId}");

                return Task.FromResult(newAdminRole);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to insert admin role mapping",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, RoleId={roleId}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> DeleteAdmin(int adminId, string userId, int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Delete admin database operation started",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sqlAdmins = Sql.Builder
                    .Select("a.adminId")
                    .From("admins a")
                    .InnerJoin("adminroles ar").On("ar.adminId = a.adminId")
                    .InnerJoin("roles r").On("r.roleId = ar.roleId")
                    .Where("ar.companyId = @0 and r.name = @1", companyId, "Administrator");

                List<int> admins = db.Fetch<int>(sqlAdmins).ToList();

                if (admins != null && admins.Count == 1)
                {
                    if (admins[0] == adminId)
                    {
                        AppLogger.Warn(
                            message: "Delete admin blocked because only one administrator remains",
                            action: "DatabaseWrite",
                            result: "Denied",
                            updatedBy: userId,
                            description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}");

                        return Task.FromResult(false);
                    }
                }
                else
                {
                    var sql = Sql.Builder
                        .Select("*")
                        .From("admins")
                        .Where("adminID = @0", adminId);

                    var admin = db.Fetch<workattendadmin>(sql).FirstOrDefault();

                    if (admin != null && admin.adminID != 0)
                    {
                        var now = DateTime.Now;
                        db.Execute("UPDATE admins SET isDeleted = 1, updatedOn = @0, updatedBy = @1 WHERE adminID = @2", now, userId, adminId);

                        AppLogger.Info(
                            message: "Admin marked as deleted successfully",
                            action: "DatabaseWrite",
                            result: "Success",
                            updatedBy: userId,
                            description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}");

                        return Task.FromResult(true);
                    }
                }

                AppLogger.Warn(
                    message: "Delete admin did not update any record",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}");

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Delete admin database operation failed",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, AdminId={adminId}, CompanyId={companyId}",
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

        public async Task<UserAccessContext?> GetUserAccessContextAsync(string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading user access context from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}");

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(databaseName))
                {
                    AppLogger.Warn(
                        message: "User access context lookup failed because userId or databaseName was empty",
                        action: "DatabaseRead",
                        result: "InvalidRequest",
                        updatedBy: userId ?? string.Empty,
                        description: $"DatabaseName={databaseName}");

                    return null;
                }

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select(@"
            ca.companyId as CompanyId,
            c.baseCompanyId as BaseCompanyId,
            ar.roleID as RoleId,
            r.policy as Policy")
                    .From("admins a")
                    .InnerJoin("companyadmin ca").On("ca.adminId = a.adminID")
                    .InnerJoin("companies c").On("c.companyId = ca.companyId")
                    .LeftJoin("adminroles ar").On("ar.adminID = a.adminID AND ar.companyID = ca.companyId")
                    .LeftJoin("roles r").On("r.roleID = ar.roleID")
                    .Where("a.adminID = @0 and a.isDeleted != 1", userId)
                    .OrderBy("ca.companyAdminId desc")
                    .Append("LIMIT 1");

                var accessContext = await db.SingleOrDefaultAsync<UserAccessContext>(sql);

                if (accessContext == null)
                {
                    AppLogger.Warn(
                        message: "User access context not found in database",
                        action: "DatabaseRead",
                        result: "NotFound",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}");

                    return null;
                }

                AppLogger.Info(
                    message: "User access context loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={accessContext.CompanyId}, BaseCompanyId={accessContext.BaseCompanyId}, RoleId={accessContext.RoleId}");

                return accessContext;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load user access context from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }
    }
}