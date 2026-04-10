using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.SubscriptionServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IUserAccessContextManager _userAccessContextManager;
        private readonly SubscriptionPaymentHelper _subscriptionPaymentHelper;

        public SubscriptionManager(
            ISubscriptionService subscriptionService,
            IUserAccessContextManager userAccessContextManager,
            SubscriptionPaymentHelper subscriptionPaymentHelper)
        {
            _subscriptionService = subscriptionService;
            _userAccessContextManager = userAccessContextManager;
            _subscriptionPaymentHelper = subscriptionPaymentHelper;
        }

        public async Task<ApiResponse<subscription>> GetPageDataAsync(CurrentUserContext ctx)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<subscription>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                int baseCompanyId = accessContext.BaseCompanyId > 0
                    ? accessContext.BaseCompanyId
                    : accessContext.CompanyId;

                var companySubs = await _subscriptionService.GetCompanyActivePackageDataAsync(baseCompanyId);
                if (companySubs == null)
                {
                    return new ApiResponse<subscription>
                    {
                        Success = false,
                        Message = "No active package found.",
                        Data = null
                    };
                }

                subscriptionpackage subsPackage = await _subscriptionService.GetActivePackageNameFromIDAsync(companySubs.SubscriptionPackageID);
                companyattribute companyAttrs = await _subscriptionService.GetCompanyAttributeAsync(baseCompanyId);

                List<subscriptionPackages> packages = new List<subscriptionPackages>();

                List<subscriptionpackage> subscriptionPackages = await _subscriptionService.GetSubscriptionPackagesAsync(subsPackage.subscriptionPackageID);
                var activePackageFeatures = await _subscriptionService.GetSubsPackageFeaturesAsync(subsPackage.subscriptionPackageID);
                var configurableFeatureId = await _subscriptionService.GetConfigurableFeturesIdAsync(baseCompanyId, companySubs.SubscriptionPackageID);

                foreach (var item in subscriptionPackages)
                {
                    subscriptionPackages packagesModel = new subscriptionPackages();

                    var packageFeatures = await _subscriptionService.GetSubsPackageFeaturesAsync(item.subscriptionPackageID);

                    packagesModel.packageID = item.subscriptionPackageID;
                    packagesModel.packageName = item.displayName;
                    packagesModel.description = item.description;
                    packagesModel.pricePerMonth = item.pricePerMonth;
                    packagesModel.pricePerYear = item.pricePerYear;
                    packagesModel.features = packageFeatures.Select(c => c.FeatureValueDetail).ToList();

                    packages.Add(packagesModel);
                }

                subscription subModel = new subscription
                {
                    currentPackageName = subsPackage.displayName,
                    packages = packages,
                    subscriptionpackagefeatures = activePackageFeatures,
                    currentPackageValid = companySubs.packageEndDate,
                    currentPackageData = companySubs,
                    address = companyAttrs,
                    configurableFeatureId = new Dictionary<int, bool>()
                };

                if (activePackageFeatures != null && activePackageFeatures.Count > 0)
                {
                    var allowedIds = new List<int>
                    {
                        (int)Constants.Features.FacePunch,
                        (int)Constants.Features.Notes,
                        (int)Constants.Features.ManualPunch,
                        (int)Constants.Features.Activity,
                        (int)Constants.Features.Job
                    };

                    subModel.configurableFeatureId = configurableFeatureId
                        .Where(feature => allowedIds.Contains(feature.SubscriptionFeatureID))
                        .ToDictionary(feature => feature.SubscriptionFeatureID, feature => feature.IsActive);
                }

                return new ApiResponse<subscription>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = subModel
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "SubscriptionManager.GetPageDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<subscription>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> ToggleSubscriptionFeatureAsync(CurrentUserContext ctx, ConfigurableFeature model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                if (model == null || model.featureId <= 0)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                int baseCompanyId = accessContext.BaseCompanyId > 0
                    ? accessContext.BaseCompanyId
                    : accessContext.CompanyId;

                await _subscriptionService.ToggleFeatureAsync(baseCompanyId, model.featureId, model.status);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "SubscriptionManager.ToggleSubscriptionFeatureAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<SiteTokenModel>> SubscribePlanAsync(CurrentUserContext ctx, subscribePlan model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<SiteTokenModel>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                if (model == null || model.packageID <= 0 || model.numberOfEmployees <= 0 || string.IsNullOrWhiteSpace(model.monthlyorAnnual))
                {
                    return new ApiResponse<SiteTokenModel>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = null
                    };
                }

                int baseCompanyId = accessContext.BaseCompanyId > 0
                    ? accessContext.BaseCompanyId
                    : accessContext.CompanyId;

                bool isYearly = false;

                PaymentTopupModel paymentModel = new PaymentTopupModel
                {
                    SubscriptionId = model.packageID
                };

                var subscriptionPackage = await _subscriptionService.GetPackageAsync(model.packageID);
                if (subscriptionPackage == null)
                {
                    return new ApiResponse<SiteTokenModel>
                    {
                        Success = false,
                        Message = "Package not found.",
                        Data = null
                    };
                }

                if (model.monthlyorAnnual.ToLower() == "month")
                {
                    paymentModel.Amount = subscriptionPackage.pricePerMonth * model.numberOfEmployees;
                    isYearly = false;
                }
                else if (model.monthlyorAnnual.ToLower() == "annual")
                {
                    paymentModel.Amount = subscriptionPackage.pricePerYear * model.numberOfEmployees;
                    paymentModel.Amount = paymentModel.Amount * 12;
                    isYearly = true;
                }
                else
                {
                    return new ApiResponse<SiteTokenModel>
                    {
                        Success = false,
                        Message = "Invalid billing cycle.",
                        Data = null
                    };
                }

                bool isPaymentStatus = false;
                if (!isPaymentStatus)
                {
                    int packageDays = 0;
                    int.TryParse(subscriptionPackage.numberOfDays, out packageDays);

                    await _subscriptionService.ConfigureNewPackageAsync(
                        baseCompanyId,
                        model.packageID,
                        packageDays,
                        isYearly);
                }

                if (!int.TryParse(accessContext.UserId?.ToString(), out int parsedUserId))
                {
                    return new ApiResponse<SiteTokenModel>
                    {
                        Success = false,
                        Message = "Invalid user id.",
                        Data = null
                    };
                }

                SiteTokenModel result = await _subscriptionPaymentHelper.TopupAsync(
                    baseCompanyId,
                    parsedUserId,
                    paymentModel.SubscriptionId,
                    paymentModel.Amount);

                return new ApiResponse<SiteTokenModel>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "SubscriptionManager.SubscribePlanAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<SiteTokenModel>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> AddAddressAsync(CurrentUserContext ctx, billingAddress model)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = false
                    };
                }

                if (model == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = false
                    };
                }

                int baseCompanyId = accessContext.BaseCompanyId > 0
                    ? accessContext.BaseCompanyId
                    : accessContext.CompanyId;

                bool isSaved = await _subscriptionService.SaveCompanyAttributeAsync(
                    baseCompanyId,
                    model,
                    accessContext.UserId.ToString(),
                    accessContext.DatabaseName);

                return new ApiResponse<bool>
                {
                    Success = isSaved,
                    Message = isSaved ? "Processed successfully." : "Something went wrong.",
                    Data = isSaved
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "SubscriptionManager.AddAddressAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = false
                };
            }
        }

        public async Task<string> ConfirmPaymentStatusAsync(string userId, string token)
        {
            return await _subscriptionPaymentHelper.ConfirmPaymentStatusAsync(userId, token);
        }

        public async Task<string> GetPaymentRedirectUrlAsync(string userId, string token)
        {
            return await _subscriptionPaymentHelper.GetPaymentRedirectUrlAsync(userId, token);
        }

        private async Task<UserAccessContext?> ResolveAccessContextAsync(CurrentUserContext ctx)
        {
            return await _userAccessContextManager.GetAsync(ctx);
        }
    }
}