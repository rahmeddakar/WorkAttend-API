using System.Collections.Generic;
using System.Threading.Tasks;
using WorkAttend.Model.Models;

namespace WorkAttend.API.Gateway.DAL.services.SubscriptionServices
{
    public interface ISubscriptionService
    {
        Task<companysubscriptionpackage> GetCompanyActivePackageDataAsync(int companyId);
        Task<subscriptionpackage> GetActivePackageNameFromIDAsync(int packageId);
        Task<companyattribute> GetCompanyAttributeAsync(int companyId);
        Task<List<subscriptionpackage>> GetSubscriptionPackagesAsync(int subscriptionPackageId);
        Task<List<SubscriptionFeatureRequestModel>> GetSubsPackageFeaturesAsync(int subscriptionPackageId);
        Task<List<SubscriptionPackageFeatureModel>> GetConfigurableFeturesIdAsync(int companyId, int subscriptionPackageId);
        Task<List<SubscriptionPackageFeatureModel>> GetCompanySubscriptionFeaturesAsync(int companyId, int featureId = 0);
        Task<subscriptionpackage> GetPackageAsync(int packageId);

        Task ToggleFeatureAsync(int companyId, int featureId, bool isActive);
        Task ConfigureNewPackageAsync(int companyId, int newPackageId, int packageDays, bool isYearly);
        Task<bool> SaveCompanyAttributeAsync(int companyId, billingAddress companyAttribute, string userId, string databaseName);

        Task<TransactionLog> SaveOrUpdateTransactionLogAsync(TransactionLog transactionLog);
        Task<TransactionLog> GetTransactionLogAsync(string userId, string sessionId, bool isRefunded = false);
        Task<PaymentTransaction> GetPaymentTransactionAsync(int paymentTransactionId);
        Task<PaymentTransaction> SavePaymentCompanyTransactionAsync(PaymentTransaction obj);

        Task<List<subscriptionfeature>> GetDefaultSubscriptionFeaturesAsync();
        Task<List<SubscriptionPackageFeatureModel>> GetSubscriptionPackageFeaturesAsync(int subscriptionPackageId);
        Task RemoveCompanyFeaturesAsync(int companyId);
        Task AddCompanyFeaturesAsync(int subscriptionPackageId, int companyId);
    }
}