using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.EmployeeServices
{
    public class EmployeesService : IEmployeeService
    {
        public Task<List<Employees>> GetAllEmployeesDataAsync(int companyId, int departmentId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading employees data from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentId={departmentId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var selectClause =
                    "e.employeeID, e.email, e.createdBy, d.departmentID, d.departmentName, c.companyId, c.name as 'companyName', " +
                    "CONCAT(e.email, '-', ep.firstname, ' ', ep.surname) as 'empDisplayName', ep.firstname, ep.surname, ep.dob, " +
                    "ep.employeecode as 'employeeCode', app.manufacturerID, app.appVersion, " +
                    "GROUP_CONCAT(ej.JobID ORDER BY ej.JobID SEPARATOR ',') as 'employeeJobs'";

                var groupByColumns =
                    "e.employeeID, e.email, e.createdBy, d.departmentID, d.departmentName, c.companyId, c.name, empDisplayName, " +
                    "ep.firstname, ep.surname, ep.dob, ep.employeecode, app.manufacturerID, app.appVersion";

                List<Employees> employees;

                if (departmentId > 0)
                {
                    var sql = Sql.Builder
                        .Select(selectClause)
                        .From("employees e")
                        .LeftJoin("employeemobileapp app").On("e.employeeid = app.employeeid and app.isDeleted != 1")
                        .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                        .InnerJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .InnerJoin("companies c").On("c.companyid = e.companyid")
                        .LeftJoin("employeejobs ej").On("ej.employeeid = e.employeeid AND ej.isActive = 1")
                        .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0 And ed.departmentID = @1", companyId, departmentId)
                        .GroupBy(groupByColumns)
                        .OrderBy("e.employeeID desc");

                    employees = db.Fetch<Employees>(sql).ToList();
                }
                else
                {
                    var sql = Sql.Builder
                        .Select(selectClause)
                        .From("employees e")
                        .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                        .LeftJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                        .LeftJoin("departments d").On("d.departmentID = ed.departmentID")
                        .LeftJoin("employeemobileapp app").On("e.employeeid = app.employeeid and app.isDeleted != 1")
                        .InnerJoin("companies c").On("c.companyid = e.companyid")
                        .LeftJoin("employeejobs ej").On("ej.employeeid = e.employeeid AND ej.isActive = 1")
                        .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0", companyId)
                        .GroupBy(groupByColumns)
                        .OrderBy("e.employeeID desc");

                    employees = db.Fetch<Employees>(sql).ToList();
                }

                AppLogger.Info(
                    message: "Employees data loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentId={departmentId}, EmployeesCount={employees.Count}");

                return Task.FromResult(employees);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load employees data from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentId={departmentId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<department>> GetCompanyDepartmentsAsync(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading company departments from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("d.departmentId, d.departmentName")
                    .From("companydepartment cd")
                    .InnerJoin("companies c").On("cd.companyid = c.companyid")
                    .InnerJoin("departments d").On("cd.departmentId = d.departmentId")
                    .Where("cd.companyId = @0 and c.isDeleted != 1 and d.isDeleted != 1", companyId);

                var departments = db.Fetch<department>(sql).ToList();

                AppLogger.Info(
                    message: "Company departments loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentsCount={departments.Count}");

                return Task.FromResult(departments);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load company departments from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<Jobs>> GetAllJobsAsync(string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading jobs from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("jobs")
                    .Where("isActive = 1");

                var jobs = db.Fetch<Jobs>(sql).ToList();

                AppLogger.Info(
                    message: "Jobs loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, JobsCount={jobs.Count}");

                return Task.FromResult(jobs);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load jobs from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<subscriptionpackagefeatures?> GetActivePackageDataAsync(int companyId, int featureId = 0)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading employee subscription package data",
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
                    message: package != null ? "Employee subscription package data loaded successfully" : "Employee subscription package data not found",
                    action: "DatabaseRead",
                    result: package != null ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}");

                return Task.FromResult(package);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load employee subscription package data",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<SubscriptionPackageFeatureModel>> GetCompanySubscriptionFeaturesAsync(int companyId, int featureId = 0)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading company employee subscription features",
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
                    message: "Company employee subscription features loaded successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}, Count={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load company employee subscription features",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyId={companyId}, FeatureId={featureId}",
                    exception: ex);

                throw;
            }
        }

        public Task<int> GetDatabaseEmployeeCountAsync(string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading database employee count",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("count(*)")
                    .From("employees e")
                    .Where("e.isDeleted != 1");

                var count = db.Fetch<int>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: "Database employee count loaded successfully",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, Count={count}");

                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load database employee count",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<employee?> GetCompanyEmployeeByEmailAsync(string email, int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading employee by email and company",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Email={email}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("employees e")
                    .Where("e.isDeleted != 1 And e.email = @0 and e.companyID = @1", email, companyId);

                var employeeData = db.Fetch<employee>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: employeeData != null ? "Employee by email and company loaded successfully" : "Employee by email and company not found",
                    action: "DatabaseRead",
                    result: employeeData != null ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Email={email}");

                return Task.FromResult(employeeData);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load employee by email and company",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Email={email}",
                    exception: ex);

                throw;
            }
        }

        public async Task<employee?> CreateEmployeeAsync(string empEmail, string empPassword, bool isMobile, int companyId, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Creating employee record in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Email={empEmail}");

                string encryptedPass = EncryptionHelper.Encrypt(empPassword);
                DateTime now = DateTime.Now;

                employee newEmployee = new employee
                {
                    email = empEmail,
                    password = encryptedPass,
                    companyID = companyId,
                    IsMobile = isMobile,
                    createdOn = now,
                    createdBy = userId,
                    updatedOn = now,
                    updatedBy = userId
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newEmployee);

                var createdEmployee = await GetCompanyEmployeeByEmailAsync(empEmail, companyId, databaseName);

                if (createdEmployee == null || createdEmployee.employeeID <= 0)
                {
                    AppLogger.Warn(
                        message: "Employee insert completed but inserted employee could not be reloaded",
                        action: "DatabaseWrite",
                        result: "Failed",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, CompanyId={companyId}, Email={empEmail}");

                    return null;
                }

                AppLogger.Info(
                    message: "Employee record created successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, EmployeeId={createdEmployee.employeeID}, Email={empEmail}");

                return createdEmployee;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to create employee record in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Email={empEmail}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> CreateEmployeeProfileAsync(int employeeId, string firstName, string surName, DateTime dob, string userId, string databaseName, string employeeCode)
        {
            try
            {
                AppLogger.Debug(
                    message: "Creating employee profile in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                DateTime now = DateTime.Now;

                employeeprofile newEmployeeProfile = new employeeprofile
                {
                    employeeID = employeeId,
                    FirstName = firstName,
                    SurName = surName,
                    DOB = dob,
                    employeeCode = employeeCode,
                    createdOn = now,
                    createdBy = userId,
                    updatedOn = now,
                    updatedBy = userId
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newEmployeeProfile);

                AppLogger.Info(
                    message: "Employee profile created successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to create employee profile in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}",
                    exception: ex);

                throw;
            }
        }

        public Task<int> GetAttributeIdAsync(string attributeName, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading employee profile attribute id from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, AttributeName={attributeName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("pa.profileAttributeID")
                    .From("profileattributes pa")
                    .Where("pa.attributeName = @0", attributeName);

                var attributeId = db.Fetch<int>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: attributeId > 0 ? "Employee profile attribute id loaded successfully" : "Employee profile attribute id not found",
                    action: "DatabaseRead",
                    result: attributeId > 0 ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, AttributeName={attributeName}, AttributeId={attributeId}");

                return Task.FromResult(attributeId);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load employee profile attribute id from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, AttributeName={attributeName}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> SaveProfileAttributeAsync(int attributeId, string value, int employeeId, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Saving employee profile attribute in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, AttributeId={attributeId}");

                DateTime now = DateTime.Now;

                employeeattribute newEmployeeAttribute = new employeeattribute
                {
                    employeeID = employeeId,
                    attributeID = attributeId,
                    Value = value,
                    createdOn = now,
                    createdBy = userId,
                    updatedOn = now,
                    updatedBy = userId
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newEmployeeAttribute);

                AppLogger.Info(
                    message: "Employee profile attribute saved successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, AttributeId={attributeId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to save employee profile attribute in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, AttributeId={attributeId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> AddEmployeeDepartmentAsync(int employeeId, int departmentId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Adding employee department mapping in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: employeeId.ToString(),
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, DepartmentId={departmentId}");

                DateTime now = DateTime.Now;

                employeedepartment newEmployeeDepartment = new employeedepartment
                {
                    employeeID = employeeId,
                    departmentID = departmentId,
                    createdOn = now,
                    createdBy = employeeId.ToString(),
                    updatedOn = now,
                    updatedBy = employeeId.ToString()
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newEmployeeDepartment);

                AppLogger.Info(
                    message: "Employee department mapping added successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: employeeId.ToString(),
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, DepartmentId={departmentId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to add employee department mapping in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: employeeId.ToString(),
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, DepartmentId={departmentId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> DeleteAllEmployeeMobileAppAsync(int employeeId, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Deleting all employee mobile app records from database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var now = DateTime.Now;
                db.Update<employeemobileapp>(
                    "SET isDeleted = 1, updatedOn = @0, updatedBy = @1 WHERE employeeID = @2",
                    now,
                    userId,
                    employeeId);

                AppLogger.Info(
                    message: "All employee mobile app records deleted successfully from database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to delete all employee mobile app records from database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> DeleteEmployeeAsync(int employeeId, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Deleting employee from database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("employees")
                    .Where("employeeid = @0", employeeId);

                var emp = db.Fetch<employee>(sql).FirstOrDefault();

                if (emp == null || emp.employeeID == 0)
                {
                    AppLogger.Warn(
                        message: "Delete employee failed because employee was not found",
                        action: "DatabaseWrite",
                        result: "NotFound",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                    return Task.FromResult(false);
                }

                var now = DateTime.Now;

                db.Update<employeeJobs>(
                    "SET isactive = 0, updatedOn = @0, updatedBy = @1 WHERE employeeID = @2",
                    now,
                    userId,
                    employeeId);

                db.Update<employee>(
                    "SET isDeleted = 1, updatedOn = @0, updatedBy = @1 WHERE employeeID = @2",
                    now,
                    userId,
                    employeeId);

                AppLogger.Info(
                    message: "Employee deleted successfully from database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to delete employee from database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> UpdateEmployeeDepartmentAsync(int employeeId, int departmentId, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating employee department in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, DepartmentId={departmentId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("employeedepartment")
                    .Where("employeeid = @0", employeeId);

                var empDept = db.Fetch<employeedepartment>(sql).FirstOrDefault();

                if (empDept == null || empDept.employeeDepartmentID == 0)
                {
                    AppLogger.Warn(
                        message: "Update employee department failed because department mapping was not found",
                        action: "DatabaseWrite",
                        result: "NotFound",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                    return Task.FromResult(false);
                }

                var now = DateTime.Now;
                db.Update<employeedepartment>(
                    "SET departmentID = @0, updatedOn = @1, updatedBy = @2 WHERE employeeDepartmentID = @3",
                    departmentId,
                    now,
                    userId,
                    empDept.employeeDepartmentID);

                AppLogger.Info(
                    message: "Employee department updated successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, DepartmentId={departmentId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update employee department in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, DepartmentId={departmentId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> UpdatePasswordAsync(int employeeId, string password, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating employee password in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                string encryptedPass = EncryptionHelper.Encrypt(password);
                var now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Update<employee>(
                    "SET password = @0, updatedOn = @1, updatedBy = @2 WHERE employeeid = @3",
                    encryptedPass,
                    now,
                    userId,
                    employeeId);

                AppLogger.Info(
                    message: "Employee password updated successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update employee password in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> UpdateFirstNameAsync(int employeeId, string firstName, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating employee first name in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                var now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Update<employeeprofile>(
                    "SET firstname = @0, updatedOn = @1, updatedBy = @2 WHERE employeeid = @3",
                    firstName,
                    now,
                    userId,
                    employeeId);

                AppLogger.Info(
                    message: "Employee first name updated successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update employee first name in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> UpdateSurNameAsync(int employeeId, string surName, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating employee surname in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                var now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Update<employeeprofile>(
                    "SET surname = @0, updatedOn = @1, updatedBy = @2 WHERE employeeid = @3",
                    surName,
                    now,
                    userId,
                    employeeId);

                AppLogger.Info(
                    message: "Employee surname updated successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update employee surname in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> UpdateEmployeeCodeAsync(int employeeId, string employeeCode, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating employee code in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                var now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Update<employeeprofile>(
                    "SET employeeCode = @0, updatedOn = @1, updatedBy = @2 WHERE employeeid = @3",
                    employeeCode,
                    now,
                    userId,
                    employeeId);

                AppLogger.Info(
                    message: "Employee code updated successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update employee code in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> InsertOrUpdateEmployeeJobsAsync(int employeeId, string employeeJobIds, bool isForAdd, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating employee jobs in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, EmployeeJobIds={employeeJobIds}");

                if (string.IsNullOrWhiteSpace(employeeJobIds))
                {
                    AppLogger.Warn(
                        message: "Updating employee jobs skipped because no job ids were provided",
                        action: "DatabaseWrite",
                        result: "InvalidRequest",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, EmployeeId={employeeId}");

                    return Task.FromResult(false);
                }

                var jobIds = employeeJobIds
                    .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.Parse(x))
                    .ToList();

                var now = DateTime.Now;

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var existingJobIds = db.Fetch<employeeJobs>(
                        "SELECT JobID as JobId FROM employeejobs WHERE employeeID = @0",
                        employeeId)
                    .Select(x => x.JobId)
                    .ToList();

                var jobsToDelete = existingJobIds.Except(jobIds).ToList();
                foreach (var jobId in jobsToDelete)
                {
                    db.Execute(
                        "UPDATE employeejobs SET isActive = 0, updatedOn = @0, updatedBy = @1 WHERE EmployeeId = @2 AND JobId = @3",
                        now,
                        userId,
                        employeeId,
                        jobId);
                }

                var jobsToInsert = jobIds.Except(existingJobIds).ToList();
                foreach (var jobId in jobsToInsert)
                {
                    employeeJobs job = new employeeJobs
                    {
                        EmployeeId = employeeId,
                        JobId = jobId,
                        isActive = true,
                        createdBy = userId,
                        createdOn = now,
                        updatedBy = userId,
                        updatedOn = now
                    };

                    db.Insert(job);
                }

                AppLogger.Info(
                    message: "Employee jobs updated successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, JobsToDelete={jobsToDelete.Count}, JobsToInsert={jobsToInsert.Count}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update employee jobs in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, EmployeeJobIds={employeeJobIds}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<Employees>> GetAllEmployeesAsync(int companyId, int departmentId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading employee locations employee list from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentId={departmentId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                List<Employees> employees;

                if (departmentId != 0)
                {
                    var sql = Sql.Builder
                        .Select("e.employeeID, e.email, e.createdBy, d.departmentName, c.companyId, c.name as 'companyName', CONCAT(e.email, '-', ep.firstname, ' ', ep.surname) as 'empDisplayName', ep.firstname, ep.surname, ep.dob")
                        .From("employees e")
                        .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                        .InnerJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                        .InnerJoin("departments d").On("d.departmentID = ed.departmentID")
                        .InnerJoin("companies c").On("c.companyid = e.companyid")
                        .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0 And ed.departmentID = @1", companyId, departmentId);

                    employees = db.Fetch<Employees>(sql).ToList();
                }
                else
                {
                    var sql = Sql.Builder
                        .Select("e.employeeID, e.email, e.createdBy, d.departmentName, c.companyId, c.name as 'companyName', CONCAT(e.email, '-', ep.firstname, ' ', ep.surname) as 'empDisplayName', ep.firstname, ep.surname, ep.dob")
                        .From("employees e")
                        .InnerJoin("employeeprofile ep").On("ep.employeeid = e.employeeid")
                        .LeftJoin("employeedepartment ed").On("ed.employeeid = e.employeeid")
                        .LeftJoin("departments d").On("d.departmentID = ed.departmentID")
                        .InnerJoin("companies c").On("c.companyid = e.companyid")
                        .Where("e.isDeleted != 1 And c.IsDeleted != 1 And e.companyId = @0", companyId);

                    employees = db.Fetch<Employees>(sql).ToList();
                }

                AppLogger.Info(
                    message: "Employee locations employee list loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentId={departmentId}, EmployeesCount={employees.Count}");

                return Task.FromResult(employees);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load employee locations employee list from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, DepartmentId={departmentId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<Location>> GetAllLocationsAsync(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading employee locations location list from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("l.locationID as Id, l.locationName as LocationName, l.locationCode as LocationCode, l.latitudeP1 as LatitudeP1, l.longitudeP1 as LongitudeP1, l.latitudeP2 as LatitudeP2, l.longitudeP2 as LongitudeP2, l.latitudeP3 as LatitudeP3, l.longitudeP3 as LongitudeP3, l.latitudeP4 as LatitudeP4, l.longitudeP4 as LongitudeP4")
                    .From("companylocations cl")
                    .InnerJoin("locations l").On("l.locationID = cl.locationID")
                    .Where("l.isDeleted != 1 and cl.companyID = @0", companyId)
                    .OrderBy("l.locationID desc");

                var locations = db.Fetch<Location>(sql).ToList();

                AppLogger.Info(
                    message: "Employee locations location list loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, LocationsCount={locations.Count}");

                return Task.FromResult(locations);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load employee locations location list from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<List<CheckinResetRule>> GetCheckinResetRulesAsync(string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading check-in reset rules from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("checkin_reset_rules")
                    .OrderBy("resetRuleID asc");

                var rules = db.Fetch<CheckinResetRule>(sql).ToList();

                AppLogger.Info(
                    message: "Check-in reset rules loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, RulesCount={rules.Count}");

                return Task.FromResult(rules);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load check-in reset rules from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}",
                    exception: ex);

                throw;
            }
        }

        public Task<int> GetCompanyCheckinResetRuleIdAsync(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading selected company check-in reset rule id from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("checkinresetruleid")
                    .From("companies")
                    .Where("companyID = @0", companyId);

                int? val = db.Fetch<int?>(sql).FirstOrDefault();
                int selectedRuleId = val.GetValueOrDefault(0);

                AppLogger.Info(
                    message: "Selected company check-in reset rule id loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, SelectedRuleId={selectedRuleId}");

                return Task.FromResult(selectedRuleId);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load selected company check-in reset rule id from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<Page<EmployeeLocationRow>> GetEmployeeLocationsPagedAsync(
            int pageIndex,
            int pageSize,
            int employeeId,
            int locationId,
            int companyId,
            string databaseName,
            string searchValue)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading paged employee locations table data from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, PageIndex={pageIndex}, PageSize={pageSize}, EmployeeId={employeeId}, LocationId={locationId}, SearchValue={searchValue}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("el.employeeLocationID, el.employeeID, e.email, ep.firstname as firstName, ep.surname as surname, el.locationID, l.locationName")
                    .From("employeelocation el")
                    .InnerJoin("locations l").On("el.locationid = l.locationid")
                    .InnerJoin("companylocations cl").On("l.locationid = cl.locationID")
                    .InnerJoin("employeeprofile ep").On("el.employeeID = ep.employeeID")
                    .InnerJoin("employees e").On("el.employeeID = e.employeeID")
                    .Where("e.isDeleted != 1 and el.isDeleted != 1 and cl.companyID = @0 and l.isDeleted = 0", companyId);

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    string searchTerm = $"%{searchValue}%";
                    sql.Where("(e.email LIKE @0 OR ep.firstname LIKE @0 OR ep.surname LIKE @0 OR l.locationName LIKE @0)", searchTerm);
                }

                if (employeeId > 0)
                {
                    sql.Where("el.EmployeeId = @0", employeeId);
                }

                if (locationId > 0)
                {
                    sql.Where("el.locationID = @0", locationId);
                }

                sql.OrderBy("el.employeeLocationID desc");

                var result = db.Page<EmployeeLocationRow>(pageIndex + 1, pageSize, sql);

                AppLogger.Info(
                    message: "Paged employee locations table data loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, TotalItems={result.TotalItems}, ReturnedItems={result.Items.Count}");

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load paged employee locations table data from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, PageIndex={pageIndex}, PageSize={pageSize}, EmployeeId={employeeId}, LocationId={locationId}, SearchValue={searchValue}",
                    exception: ex);

                throw;
            }
        }
        public Task<EmployeesLocation?> GetSingleEmployeeLocationAsync(int employeeId, int locationId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading single employee location mapping from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, LocationId={locationId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("*")
                    .From("employeelocation")
                    .Where("EmployeeId = @0 and locationID = @1 and isDeleted != 1", employeeId, locationId);

                var data = db.Fetch<EmployeesLocation>(sql).FirstOrDefault();

                AppLogger.Info(
                    message: data != null ? "Single employee location mapping loaded successfully" : "Single employee location mapping not found",
                    action: "DatabaseRead",
                    result: data != null ? "Success" : "NotFound",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, LocationId={locationId}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load single employee location mapping from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, LocationId={locationId}",
                    exception: ex);

                throw;
            }
        }

        public async Task<bool> CreateEmployeeLocationAsync(int employeeId, int locationId, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Creating employee location mapping in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, LocationId={locationId}");

                var existing = await GetSingleEmployeeLocationAsync(employeeId, locationId, databaseName);
                if (existing != null && existing.employeeLocationID > 0)
                {
                    AppLogger.Warn(
                        message: "Create employee location mapping skipped because record already exists",
                        action: "DatabaseWrite",
                        result: "Duplicate",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, LocationId={locationId}");

                    return false;
                }

                DateTime now = DateTime.Now;

                EmployeesLocation newEmployeeLocation = new EmployeesLocation
                {
                    employeeID = employeeId,
                    LocationID = locationId,
                    createdBy = userId,
                    createdOn = now,
                    updatedBy = userId,
                    updatedOn = now,
                    isDeleted = false
                };

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                db.Insert(newEmployeeLocation);

                AppLogger.Info(
                    message: "Employee location mapping created successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, LocationId={locationId}");

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to create employee location mapping in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, EmployeeId={employeeId}, LocationId={locationId}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> DeleteEmployeeLocationAsync(int employeeId, string locationName, int companyId, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Deleting employee location mapping from database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, EmployeeId={employeeId}, LocationName={locationName}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("l.locationID")
                    .From("locations l")
                    .InnerJoin("companylocations cl").On("l.locationID = cl.locationID")
                    .Where("l.locationName = @0 and l.isDeleted != 1 and cl.companyID = @1", locationName, companyId);

                int locationId = db.Fetch<int>(sql).FirstOrDefault();

                if (locationId <= 0)
                {
                    AppLogger.Warn(
                        message: "Delete employee location mapping failed because location was not found for company",
                        action: "DatabaseWrite",
                        result: "NotFound",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, CompanyId={companyId}, EmployeeId={employeeId}, LocationName={locationName}");

                    return Task.FromResult(false);
                }

                var now = DateTime.Now;

                int rowsAffected = db.Execute(
                    "UPDATE employeelocation SET isDeleted = 1, updatedOn = @0, updatedBy = @1 WHERE employeeID = @2 and locationID = @3 and isDeleted != 1",
                    now,
                    userId,
                    employeeId,
                    locationId);

                bool isDeleted = rowsAffected > 0;

                if (!isDeleted)
                {
                    AppLogger.Warn(
                        message: "Delete employee location mapping failed because no active record was found",
                        action: "DatabaseWrite",
                        result: "NotFound",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, CompanyId={companyId}, EmployeeId={employeeId}, LocationId={locationId}, LocationName={locationName}");

                    return Task.FromResult(false);
                }

                AppLogger.Info(
                    message: "Employee location mapping deleted successfully from database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, EmployeeId={employeeId}, LocationId={locationId}, LocationName={locationName}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to delete employee location mapping from database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, EmployeeId={employeeId}, LocationName={locationName}",
                    exception: ex);

                throw;
            }
        }

        public Task<bool> SetCompanyCheckinResetRuleIdAsync(int companyId, int ruleId, string userId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Updating company check-in reset rule in database",
                    action: "DatabaseWrite",
                    result: "Started",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, RuleId={ruleId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var now = DateTime.Now;

                int rowsAffected = db.Execute(
                        "UPDATE companies SET checkinresetruleid = @0, updatedOn = @1, updatedBy = @2 WHERE companyID = @3",
                        ruleId,
                        now,
                        userId,
                        companyId);

                bool isSaved = rowsAffected > 0;

                if (!isSaved)
                {
                    AppLogger.Warn(
                        message: "Updating company check-in reset rule failed because no company record was updated",
                        action: "DatabaseWrite",
                        result: "Failed",
                        updatedBy: userId,
                        description: $"DatabaseName={databaseName}, CompanyId={companyId}, RuleId={ruleId}");

                    return Task.FromResult(false);
                }

                AppLogger.Info(
                    message: "Company check-in reset rule updated successfully in database",
                    action: "DatabaseWrite",
                    result: "Success",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, RuleId={ruleId}");

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to update company check-in reset rule in database",
                    action: "DatabaseWrite",
                    result: "Failed",
                    updatedBy: userId,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, RuleId={ruleId}",
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