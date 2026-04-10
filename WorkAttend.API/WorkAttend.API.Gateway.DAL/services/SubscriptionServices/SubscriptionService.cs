using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.SubscriptionServices
{
    public class SubscriptionService : ISubscriptionService
    {
        public async Task<companysubscriptionpackage> GetCompanyActivePackageDataAsync(int companyId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("companysubscriptionpackages")
                .Where("isActive = 1 and isDeleted = 0 and companyID = @0", companyId);

            return await Task.FromResult(context.Fetch<companysubscriptionpackage>(ppSql).FirstOrDefault());
        }

        public async Task<subscriptionpackage> GetActivePackageNameFromIDAsync(int packageId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("subscriptionpackages")
                .Where("subscriptionpackageID = @0 and isDeleted != 1", packageId);

            return await Task.FromResult(context.Fetch<subscriptionpackage>(ppSql).FirstOrDefault());
        }

        public async Task<companyattribute> GetCompanyAttributeAsync(int companyId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("companyattribute")
                .Where("companyID = @0", companyId);

            return await Task.FromResult(context.Fetch<companyattribute>(ppSql).FirstOrDefault());
        }

        public async Task<List<subscriptionpackage>> GetSubscriptionPackagesAsync(int subscriptionPackageId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("subscriptionpackages")
                .Where("isDeleted = 0 and isSystem != 1 and subscriptionPackageID != @0", subscriptionPackageId);

            return await Task.FromResult(context.Fetch<subscriptionpackage>(ppSql).ToList());
        }

        public async Task<List<SubscriptionFeatureRequestModel>> GetSubsPackageFeaturesAsync(int subscriptionPackageId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("sf.FeatureName, sf.DisplayName, sf.FeatureDescription, sf.SubscriptionFeatureID, spf.FeatureValue, spf.FeatureValueDetail, spf.IsActive")
                .From("subscriptionpackagefeatures spf")
                .InnerJoin("subscriptionfeatures sf").On("sf.SubscriptionFeatureID = spf.SubscriptionFeatureID")
                .Where("spf.isactive = 1 and sf.IsActive = 1 and spf.SubscriptionPackageID = @0 and spf.IsDeleted = 0 and sf.IsDeleted = 0 and sf.IsActive = 1", subscriptionPackageId);

            return await Task.FromResult(context.Fetch<SubscriptionFeatureRequestModel>(ppSql).ToList());
        }

        public async Task<List<SubscriptionPackageFeatureModel>> GetConfigurableFeturesIdAsync(int companyId, int subscriptionPackageId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("SubscriptionPackageID, subscriptionpackagefeatureId as SubscriptionFeatureID, IsActive")
                .From("companysubscriptionpackagefeatures")
                .Where("companyid = @0 and subscriptionpackageId = @1 and isDelete = 0", companyId, subscriptionPackageId);

            return await Task.FromResult(context.Fetch<SubscriptionPackageFeatureModel>(ppSql).ToList());
        }

        public async Task<List<SubscriptionPackageFeatureModel>> GetCompanySubscriptionFeaturesAsync(int companyId, int featureId = 0)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("cspf.subscriptionpackagefeatureId as SubscriptionFeatureID, cspf.IsActive, cspf.FeatureValue")
                .From("companysubscriptionpackagefeatures cspf");

            if (featureId > 0)
                ppSql.Where("cspf.CompanyId = @0 and cspf.subscriptionpackagefeatureid = @1 and cspf.isDelete = 0", companyId, featureId);
            else
                ppSql.Where("cspf.CompanyId = @0 and cspf.isDelete = 0", companyId);

            return await Task.FromResult(context.Fetch<SubscriptionPackageFeatureModel>(ppSql).ToList());
        }

        public async Task<subscriptionpackage> GetPackageAsync(int packageId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("subscriptionpackages")
                .Where("subscriptionpackageID = @0 and isDeleted != 1", packageId);

            return await Task.FromResult(context.Fetch<subscriptionpackage>(ppSql).FirstOrDefault());
        }

        public async Task ToggleFeatureAsync(int companyId, int featureId, bool isActive)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("companysubscriptionpackagefeatures")
                .Where("companyId = @0 and subscriptionpackagefeatureId = @1 and isDelete = 0", companyId, featureId);

            var features = context.Fetch<companysubscriptionpackagefeatures>(ppSql).ToList();

            if (features != null && features.Count > 0)
            {
                foreach (var item in features)
                {
                    item.isActive = isActive;
                    item.updatedOn = DateTime.Now;
                    item.updatedBy = companyId.ToString();
                    context.Update(item);
                }
            }

            await Task.CompletedTask;
        }

        public async Task ConfigureNewPackageAsync(int companyId, int newPackageId, int packageDays, bool isYearly)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            DateTime now = DateTime.Now;
            DateTime endDate = isYearly ? now.AddDays(365) : now.AddDays(packageDays);

            var oldPackageSql = Sql.Builder
                .Select("*")
                .From("companysubscriptionpackages")
                .Where("companyId = @0 and isDeleted = 0", companyId);

            var oldPackages = context.Fetch<companysubscriptionpackage>(oldPackageSql).ToList();

            foreach (var oldPackage in oldPackages)
            {
                oldPackage.IsDeleted = true;
                oldPackage.IsActive = false;
                oldPackage.UpdatedOn = now;
                oldPackage.UpdatedBy = companyId;
                context.Update(oldPackage);
            }

            companysubscriptionpackage newCompSubs = new companysubscriptionpackage
            {
                companyID = companyId,
                SubscriptionPackageID = newPackageId,
                packageStartDate = now,
                packageEndDate = endDate,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = companyId,
                CreatedOn = now,
                UpdatedOn = now,
                UpdatedBy = companyId
            };

            object newId = context.Insert(newCompSubs);
            newCompSubs.companySubscriptionPackageID = Convert.ToInt32(newId);

            await RemoveCompanyFeaturesAsync(companyId);
            await AddCompanyFeaturesAsync(newPackageId, companyId);
        }

        public async Task<bool> SaveCompanyAttributeAsync(int companyId, billingAddress companyAttribute, string userId, string databaseName)
        {
            DateTime now = DateTime.Now;
            companyattribute companyAttr = await GetCompanyAttributeAsync(companyId);

            var context = DataContextHelper.GetWorkAttendBaseContext();

            if (companyAttr != null && companyAttr.companyAttributeID > 0)
            {
                companyAttr.AddressLine1 = companyAttribute.AddressLine1;
                companyAttr.AddressLine2 = companyAttribute.AddressLine2;
                companyAttr.currencyID = companyAttribute.currencyID;
                companyAttr.city = companyAttribute.city;
                companyAttr.phoneNumber = companyAttribute.phoneNumber;
                companyAttr.postcode = companyAttribute.postcode;
                companyAttr.updatedBy = userId;
                companyAttr.updatedOn = now;

                context.Update(companyAttr);
                return await Task.FromResult(true);
            }

            companyattribute newCompanyAttribute = new companyattribute
            {
                companyID = companyId,
                AddressLine1 = companyAttribute.AddressLine1,
                AddressLine2 = companyAttribute.AddressLine2,
                currencyID = companyAttribute.currencyID,
                city = companyAttribute.city,
                phoneNumber = companyAttribute.phoneNumber,
                postcode = companyAttribute.postcode,
                createdBy = userId,
                updatedBy = userId,
                createdOn = now,
                updatedOn = now
            };

            object id = context.Insert(newCompanyAttribute);
            newCompanyAttribute.companyAttributeID = Convert.ToInt32(id);

            return await Task.FromResult(true);
        }

        public async Task<TransactionLog> SaveOrUpdateTransactionLogAsync(TransactionLog transactionLog)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            if (transactionLog.TransactionLogId > 0)
            {
                context.Update(transactionLog);
            }
            else
            {
                var id = context.Insert(transactionLog);
                transactionLog.TransactionLogId = Convert.ToInt32(id);
            }

            return await Task.FromResult(transactionLog);
        }

        public async Task<TransactionLog> GetTransactionLogAsync(string userId, string sessionId, bool isRefunded = false)
        {
            int parsedUserId = 0;
            int.TryParse(userId, out parsedUserId);

            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("TransactionLogs")
                .Where("UserId = @0 and ReferenceCode = @1 and IsCompleted = 0", parsedUserId, sessionId);

            return await Task.FromResult(context.Fetch<TransactionLog>(ppSql).FirstOrDefault());
        }

        public async Task<PaymentTransaction> GetPaymentTransactionAsync(int paymentTransactionId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("PaymentTransactions")
                .Where("PaymentTransactionId = @0", paymentTransactionId);

            return await Task.FromResult(context.Fetch<PaymentTransaction>(ppSql).FirstOrDefault());
        }

        public async Task<PaymentTransaction> SavePaymentCompanyTransactionAsync(PaymentTransaction obj)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var id = context.Insert(obj);
            obj.PaymentTransactionId = Convert.ToInt32(id);

            return await Task.FromResult(obj);
        }

        public async Task<List<subscriptionfeature>> GetDefaultSubscriptionFeaturesAsync()
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("subscriptionfeatures")
                .Where("IsDefault = 1 and IsActive = 1");

            return await Task.FromResult(context.Fetch<subscriptionfeature>(ppSql).ToList());
        }

        public async Task<List<SubscriptionPackageFeatureModel>> GetSubscriptionPackageFeaturesAsync(int subscriptionPackageId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("SubscriptionFeatureID, FeatureValue, IsActive")
                .From("subscriptionpackagefeatures")
                .Where("SubscriptionPackageID = @0 and IsDeleted = 0", subscriptionPackageId);

            return await Task.FromResult(context.Fetch<SubscriptionPackageFeatureModel>(ppSql).ToList());
        }

        public async Task RemoveCompanyFeaturesAsync(int companyId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var ppSql = Sql.Builder
                .Select("*")
                .From("companysubscriptionpackagefeatures")
                .Where("companyId = @0 and isDelete = 0", companyId);

            var existing = context.Fetch<companysubscriptionpackagefeatures>(ppSql).ToList();

            foreach (var item in existing)
            {
                item.IsDelete = true;
                item.isActive = false;
                item.updatedOn = DateTime.Now;
                item.updatedBy = "system";
                context.Update(item);
            }

            await Task.CompletedTask;
        }

        public async Task AddCompanyFeaturesAsync(int subscriptionPackageId, int companyId)
        {
            var context = DataContextHelper.GetWorkAttendBaseContext();

            var features = await GetDefaultSubscriptionFeaturesAsync();

            if (features != null && features.Count > 0)
            {
                var packageFeatures = await GetSubscriptionPackageFeaturesAsync(subscriptionPackageId);

                foreach (var feature in features)
                {
                    bool isFeatureActive = packageFeatures
                        .Where(c => c.SubscriptionFeatureID == feature.SubscriptionFeatureID)
                        .Select(c => c.IsActive)
                        .FirstOrDefault();

                    int featureValue = packageFeatures
                        .Where(c => c.SubscriptionFeatureID == feature.SubscriptionFeatureID)
                        .Select(c => c.FeatureValue)
                        .FirstOrDefault();

                    companysubscriptionpackagefeatures companyFeature = new companysubscriptionpackagefeatures
                    {
                        companyId = companyId,
                        subscriptionpackageId = subscriptionPackageId,
                        subscriptionpackagefeatureId = feature.SubscriptionFeatureID,
                        FeatureValue = featureValue,
                        isActive = isFeatureActive,
                        createdBy = "system",
                        createdOn = DateTime.Now,
                        IsDelete = false
                    };

                    context.Insert(companyFeature);
                }
            }
        }
    }
}