using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.DAL.services.DepartmentServices
{
    public class DepartmentService : IDepartmentService
    {
        public Task<List<department>> GetAllDepartmentsAsync(int companyId, string databaseName)
        {
            try
            {
                AppLogger.Debug(
                    message: "Loading departments from database",
                    action: "DatabaseRead",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}");

                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("d.*")
                    .From("companydepartment cd")
                    .InnerJoin("companies c").On("cd.companyid = c.companyid")
                    .InnerJoin("departments d").On("cd.departmentId = d.departmentId")
                    .Where("cd.companyId = @0 and c.isDeleted != 1 and d.isDeleted != 1", companyId)
                    .OrderBy("d.departmentId desc");

                var data = db.Fetch<department>(sql).ToList();

                AppLogger.Info(
                    message: "Departments loaded successfully from database",
                    action: "DatabaseRead",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}, Count={data.Count}");

                return Task.FromResult(data);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Failed to load departments from database",
                    action: "DatabaseRead",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"DatabaseName={databaseName}, CompanyId={companyId}",
                    exception: ex);

                throw;
            }
        }

        public Task<int> GetDatabaseDepartmentCountAsync(string databaseName)
        {
            try
            {
                var repository = DataContextHelper.GetCompanyDataContext(databaseName);
                using var db = repository.GetDatabase();

                var sql = Sql.Builder
                    .Select("count(*)")
                    .From("departments d");

                int count = db.Fetch<int>(sql).FirstOrDefault();
                return Task.FromResult(count);
            }
            catch
            {
                throw;
            }
        }

        public Task<department?> CheckDepartmentExistAsync(string departmentCode, int companyId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var departmentSql = Sql.Builder
                .Select("*")
                .From("departments")
                .Where("departmentCode = @0", departmentCode);

            var dept = db.Fetch<department>(departmentSql).FirstOrDefault();

            if (dept == null || dept.departmentID <= 0)
                return Task.FromResult<department?>(null);

            var companyDepartmentSql = Sql.Builder
                .Select("*")
                .From("companydepartment")
                .Where("departmentid = @0 and companyid = @1", dept.departmentID, companyId);

            var companyDept = db.Fetch<companydepartment>(companyDepartmentSql).FirstOrDefault();

            if (companyDept != null && companyDept.departmentID > 0)
                return Task.FromResult<department?>(dept);

            return Task.FromResult<department?>(null);
        }

        public Task<department> CreateDepartmentAsync(string departmentName, string departmentCode, string userId, string databaseName)
        {
            DateTime now = DateTime.Now;

            department newDepartment = new department
            {
                departmentName = departmentName,
                departmentCode = departmentCode,
                createdOn = now,
                createdBy = userId,
                updatedOn = now,
                updatedBy = userId,
                isDeleted = false
            };

            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            object id = db.Insert(newDepartment);
            newDepartment.departmentID = int.Parse(id.ToString()!);

            return Task.FromResult(newDepartment);
        }

        public Task<bool> UpdateDepartmentAsync(int departmentId, string departmentCode, string departmentName, string userId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var now = DateTime.Now;

            db.Update<department>(
                "SET departmentCode = @0, departmentName = @1, updatedOn = @2, updatedBy = @3 WHERE departmentID = @4",
                departmentCode,
                departmentName,
                now,
                userId,
                departmentId);

            return Task.FromResult(true);
        }

        public Task<int> GetEmployeeDepartmentUsageCountAsync(string databaseName, int departmentId)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var sql = Sql.Builder
                .Select("count(*)")
                .From("employeedepartment")
                .Where("departmentID = @0", departmentId);

            int count = db.Fetch<int>(sql).FirstOrDefault();
            return Task.FromResult(count);
        }

        public Task<bool> DeleteDepartmentAsync(int departmentId, string userId, string databaseName)
        {
            var repository = DataContextHelper.GetCompanyDataContext(databaseName);
            using var db = repository.GetDatabase();

            var now = DateTime.Now;

            db.Update<department>(
                "SET isDeleted = 1, updatedOn = @0, updatedBy = @1 WHERE departmentId = @2",
                now,
                userId,
                departmentId);

            return Task.FromResult(true);
        }
    }
}