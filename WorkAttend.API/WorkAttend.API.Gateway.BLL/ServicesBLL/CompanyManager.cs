using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkAttend.API.Gateway.BLL.InterfaceBLL;
using WorkAttend.API.Gateway.DAL.Common.Helper;
using WorkAttend.API.Gateway.DAL.services.AdminsServices;
using WorkAttend.API.Gateway.DAL.services.CompanyServices;
using WorkAttend.API.Gateway.DAL.services.DepartmentServices;
using WorkAttend.Model.Models;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Helpers;

namespace WorkAttend.API.Gateway.BLL.ServicesBLL
{
    public class CompanyManager : ICompanyManager
    {
        private const int DefaultDepartmentId = 1;
        private const int DefaultAdministratorRoleId = 1;

        private readonly ICompanyService _companyService;
        private readonly CompanyRegistrationHelper _companyRegistrationHelper;
        private readonly IAdminsService _adminsService;
        private readonly IConfiguration _configuration;
        private readonly IDepartmentService _departmentService;
        private readonly IUserAccessContextManager _userAccessContextManager;

        public CompanyManager(
            ICompanyService companyService,
            CompanyRegistrationHelper companyRegistrationHelper,
            IAdminsService adminsService,
            IConfiguration configuration,
            IDepartmentService departmentService,
            IUserAccessContextManager userAccessContextManager)
        {
            _companyService = companyService;
            _companyRegistrationHelper = companyRegistrationHelper;
            _adminsService = adminsService;
            _configuration = configuration;
            _departmentService = departmentService;
            _userAccessContextManager = userAccessContextManager;
        }

        public async Task<ApiResponse<RegisterCompanyPageData>> GetRegisterDataAsync()
        {
            try
            {
                AppLogger.Info(
                    message: "Register data load started",
                    action: "Read",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: string.Empty);

                var countries = await _companyRegistrationHelper.GetCountriesAsync();
                var currencies = await _companyRegistrationHelper.GetCurrenciesAsync();
                var industries = await _companyRegistrationHelper.GetIndustriesAsync();

                var pageData = new RegisterCompanyPageData
                {
                    countries = countries,
                    currencies = currencies,
                    industries = industries
                };

                AppLogger.Info(
                    message: "Register data load completed successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"Countries={countries.Count}, Currencies={currencies.Count}, Industries={industries.Count}");

                return new ApiResponse<RegisterCompanyPageData>
                {
                    Success = true,
                    Message = "Processed successfully.",
                    Data = pageData
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Register data load failed",
                    action: "Read",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: string.Empty,
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "CompanyManager.GetRegisterDataAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<RegisterCompanyPageData>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<CompanyRegistrationCheckResponse>> CompanyURLExistAsync(CompanyRegistrationCheckRequest model)
        {
            try
            {
                AppLogger.Info(
                    message: "Company registration check started",
                    action: "Read",
                    result: "Started",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={model?.companyURL}, PENumber={model?.peNumber}");

                var companyconfigs = await _companyRegistrationHelper.GetCompanyConfigurationsAsync(model?.companyURL ?? string.Empty);
                var companyList = await _companyRegistrationHelper.CheckCompanyExistAsync(model?.peNumber ?? string.Empty);

                string message = string.Empty;
                bool urlAlreadyExist = false;
                bool isExist = false;

                if (companyconfigs != null && companyconfigs.companyConfigID > 0)
                {
                    isExist = true;
                    urlAlreadyExist = true;
                    message = "Company URL already in-use.";
                }
                else if (companyList != null && companyList.Count > 0)
                {
                    isExist = true;
                    message = "Company registration number already exist.";
                }

                AppLogger.Info(
                    message: "Company registration check completed successfully",
                    action: "Read",
                    result: "Success",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={model?.companyURL}, PENumber={model?.peNumber}, IsExist={isExist}, UrlAlreadyExist={urlAlreadyExist}");

                return new ApiResponse<CompanyRegistrationCheckResponse>
                {
                    Success = true,
                    Message = message,
                    Data = new CompanyRegistrationCheckResponse
                    {
                        isExist = isExist,
                        urlAlreadyExist = urlAlreadyExist
                    }
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Company registration check failed",
                    action: "Read",
                    result: "Failed",
                    updatedBy: string.Empty,
                    description: $"CompanyURL={model?.companyURL}, PENumber={model?.peNumber}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "CompanyManager.CompanyURLExistAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<CompanyRegistrationCheckResponse>
                {
                    Success = false,
                    Message = "Something went wrong.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<RegisterCompanyResult>> RegisterCompanyAsync(registerCompany registerModel)
        {
            string userGeneratedPassword = StringHelper.GetRandomAlphanumericString(8);
            string message = string.Empty;

            try
            {
                AppLogger.Info(
                    message: "Company registration started",
                    action: "Create",
                    result: "Started",
                    updatedBy: registerModel?.adminEmail ?? string.Empty,
                    description: $"CompanyName={registerModel?.companyName}, CompanyURL={registerModel?.companyURL}");

                if (!IsValidRegisterRequest(registerModel))
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "Kindly fill all fields correctly to continue.",
                        Data = null
                    };
                }

                registerModel.adminPassword = userGeneratedPassword;

                var companyList = await _companyRegistrationHelper.CheckCompanyExistAsync(registerModel.peNumber);
                var companyconfigs = await _companyRegistrationHelper.GetCompanyConfigurationsAsync(registerModel.companyURL);

                if (companyList != null && companyList.Count > 0)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "company already exist.",
                        Data = null
                    };
                }

                if (companyconfigs != null && companyconfigs.companyConfigID > 0)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "company already exist.",
                        Data = null
                    };
                }

                bool databaseCreated = false;
                string databaseName = string.Empty;

                var dbManager = await _companyRegistrationHelper.GetAvailableDbAsync();
                if (dbManager != null && dbManager.databasemanagerID > 0)
                {
                    bool assigned = await _companyRegistrationHelper.AssignDatabaseAsync(dbManager.databasemanagerID, registerModel.companyName);
                    if (!assigned)
                    {
                        return new ApiResponse<RegisterCompanyResult>
                        {
                            Success = false,
                            Message = "Something went wrong. Try again creating a company.",
                            Data = null
                        };
                    }

                    databaseName = dbManager.databaseName;
                    databaseCreated = true;
                }
                else
                {
                    databaseName = BuildDatabaseName(registerModel.companyName);

                    bool created = await _companyRegistrationHelper.CreateCompanyDataBaseAsync(databaseName);
                    if (!created)
                    {
                        return new ApiResponse<RegisterCompanyResult>
                        {
                            Success = false,
                            Message = "Something went wrong. Try again creating a company.",
                            Data = null
                        };
                    }

                    bool tablesCreated = await _companyRegistrationHelper.CopyAllTablesAsync(databaseName);
                    if (!tablesCreated)
                    {
                        return new ApiResponse<RegisterCompanyResult>
                        {
                            Success = false,
                            Message = "Something went wrong. Try again creating a company.",
                            Data = null
                        };
                    }

                    databaseCreated = true;
                }

                if (!databaseCreated)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating a company.",
                        Data = null
                    };
                }

                int timezoneId = await _companyRegistrationHelper.GetTimeZoneIdFromNameAsync(TimeZoneInfo.Local.StandardName);

                var newCompany = await _companyService.CreateCompanyAsync(
                    registerModel.companyName.ToUpperInvariant(),
                    registerModel.peNumber,
                    registerModel.isDakarConnected,
                    timezoneId);

                if (newCompany == null || newCompany.companyId <= 0)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating a company.",
                        Data = null
                    };
                }

                string countryName = await _companyRegistrationHelper.GetCountryNameByIdAsync(registerModel.countryID);

                await _companyService.AddCompanyAttrAsync(
                    newCompany.companyId,
                    registerModel.adminEmail,
                    registerModel.countryID,
                    countryName,
                    registerModel.phoneNumber,
                    registerModel.companyName,
                    registerModel.adminEmail,
                    registerModel.CurrencyTypeID,
                    registerModel.accountType,
                    registerModel.expectedNumOfEmp);

                string mobileApplicationKey = await _companyRegistrationHelper.GenerateUniqueMobileApplicationKeyAsync(registerModel.companyName);

                await _companyService.CreateCompanyConfigAsync(
                    newCompany.companyId,
                    databaseName,
                    registerModel.industryID,
                    registerModel.description,
                    registerModel.phoneNumber,
                    registerModel.companyURL,
                    mobileApplicationKey);

                var package = await _companyRegistrationHelper.GetSubscriptionByNameAsync("freetrial");
                if (package == null || package.subscriptionPackageID <= 0)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating a company.",
                        Data = null
                    };
                }

                await _companyRegistrationHelper.CreateCompanySubscriptionAsync(package.subscriptionPackageID, newCompany.companyId);
                await _companyRegistrationHelper.AddCompanyFeaturesAsync(package.subscriptionPackageID, newCompany.companyId);

                var companyInNewDB = await _companyService.CreateCompanyNewDBAsync(
                    registerModel.companyName,
                    registerModel.peNumber,
                    registerModel.isDakarConnected,
                    databaseName,
                    newCompany.companyId,
                    timezoneId);

                if (companyInNewDB == null || companyInNewDB.companyId <= 0)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating a company.",
                        Data = null
                    };
                }

                string adminName = $"{registerModel.firstName} {registerModel.lastName}".Trim();

                var newAdmin = await _adminsService.AddAdmin(
                    registerModel.adminEmail,
                    registerModel.adminPassword,
                    registerModel.adminEmail,
                    databaseName,
                    adminName);

                if (newAdmin == null || newAdmin.adminID <= 0)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating a company.",
                        Data = null
                    };
                }

                var newCompanyAdmin = await _adminsService.AddCompanyAdmin(
                    newAdmin.adminID,
                    companyInNewDB.companyId,
                    databaseName);

                if (newCompanyAdmin == null || newCompanyAdmin.companyAdminId <= 0)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating a company.",
                        Data = null
                    };
                }

                await _adminsService.AddAdminRole(
                    newAdmin.adminID,
                    DefaultAdministratorRoleId,
                    registerModel.adminEmail,
                    databaseName,
                    companyInNewDB.companyId);

                await _companyService.CreateCompanyDepartmentAsync(
                    DefaultDepartmentId,
                    companyInNewDB.companyId,
                    newAdmin.adminID.ToString(),
                    databaseName);

                string configuredBaseUrl =
                    _configuration["AppSettings:companyBaseURL"] ??
                    _configuration["companyBaseURL"] ??
                    string.Empty;

                configuredBaseUrl = configuredBaseUrl.Trim().TrimEnd('/');
                string companyLoginUrl = $"https://{registerModel.companyURL}.{configuredBaseUrl}/login";

                bool emailSent = await EmailHelper.SendRegisterEmail(
                    _configuration,
                    registerModel.adminEmail,
                    adminName,
                    registerModel.companyName,
                    userGeneratedPassword,
                    companyLoginUrl);

                if (!emailSent)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "Please contact admin. Error sending emails.",
                        Data = null
                    };
                }

                string textMessage = $"Hi {registerModel.firstName}, Thank you for registering with us. We have sent your login credentials to your email address.";
                await MessageHelper.SendMessageToNumber(_configuration, textMessage, registerModel.phoneNumber);

                bool isSubscriptionValid = await _companyRegistrationHelper.GetActivePackageAsync(newCompany.companyId);

                if (!isSubscriptionValid)
                {
                    return new ApiResponse<RegisterCompanyResult>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating a company.",
                        Data = null
                    };
                }

                AppLogger.Info(
                    message: "Company registration completed successfully",
                    action: "Create",
                    result: "Success",
                    updatedBy: registerModel.adminEmail,
                    description: $"CompanyId={newCompany.companyId}, DatabaseName={databaseName}, CompanyURL={registerModel.companyURL}");

                return new ApiResponse<RegisterCompanyResult>
                {
                    Success = true,
                    Message = "Company is created successfully.",
                    Data = new RegisterCompanyResult
                    {
                        companyBaseURL = companyLoginUrl
                    }
                };
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    message: "Company registration failed",
                    action: "Create",
                    result: "Failed",
                    updatedBy: registerModel?.adminEmail ?? string.Empty,
                    description: $"CompanyName={registerModel?.companyName}, CompanyURL={registerModel?.companyURL}",
                    exception: ex);

                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "CompanyManager.RegisterCompanyAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                message = "Something went wrong. Try again creating a company.";

                return new ApiResponse<RegisterCompanyResult>
                {
                    Success = false,
                    Message = message,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<RegisterNewCompanyResult>> RegisterNewCompanyAsync(CurrentUserContext ctx, registerNewCompany registerModel)
        {
            try
            {
                var accessContext = await ResolveAccessContextAsync(ctx);
                if (accessContext == null)
                {
                    return new ApiResponse<RegisterNewCompanyResult>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        Data = null
                    };
                }

                var companySub = await _adminsService.GetActivePackageData(accessContext.BaseCompanyId, (int)Constants.Features.Company);
                var companySubscriptionPackageFeatureValue = await _adminsService.GetCompanySubscriptionFeatures(accessContext.BaseCompanyId, (int)Constants.Features.Company);
                int currentCompanyCount = await _companyService.GetDatabaseCompanyCountAsync(accessContext.DatabaseName);

                if (companySub == null)
                {
                    return new ApiResponse<RegisterNewCompanyResult>
                    {
                        Success = false,
                        Message = "Subscription information not found.",
                        Data = null
                    };
                }

                int maxCompanyCount =
                    companySubscriptionPackageFeatureValue != null && companySubscriptionPackageFeatureValue.Count > 0
                        ? companySubscriptionPackageFeatureValue[0].FeatureValue
                        : companySub.FeatureValue;

                if (!(currentCompanyCount < maxCompanyCount || companySub.FeatureValue == -1))
                {
                    return new ApiResponse<RegisterNewCompanyResult>
                    {
                        Success = false,
                        Message = "Subscription limit reached.",
                        Data = null
                    };
                }

                var permissionActions = PermissionHelper.GetAllowedActions(accessContext.Policy, "newcompany");
                bool isAllowed = permissionActions.Contains(ActionTypeEnum.Create.ToString().ToLower());

                if (!isAllowed)
                {
                    return new ApiResponse<RegisterNewCompanyResult>
                    {
                        Success = false,
                        Message = "Permission not allowed.",
                        Data = null
                    };
                }

                if (registerModel == null ||
                    string.IsNullOrWhiteSpace(registerModel.companyName) ||
                    string.IsNullOrWhiteSpace(registerModel.peNumber))
                {
                    return new ApiResponse<RegisterNewCompanyResult>
                    {
                        Success = false,
                        Message = "Kindly fill all fields correctly to continue.",
                        Data = null
                    };
                }

                var admin = await _companyService.CheckAdminExistAsync(ctx.Email, accessContext.DatabaseName);
                if (admin == null || admin.adminID <= 0)
                {
                    return new ApiResponse<RegisterNewCompanyResult>
                    {
                        Success = false,
                        Message = "Unable to find current admin.",
                        Data = null
                    };
                }

                var companyList = await _companyService.CheckCompanyExistAsync(registerModel.peNumber);
                if (companyList != null && companyList.Count > 0)
                {
                    return new ApiResponse<RegisterNewCompanyResult>
                    {
                        Success = false,
                        Message = "company already exist.",
                        Data = null
                    };
                }

                var currentCompany = await _companyService.GetCompanyByIdAsync(accessContext.CompanyId, accessContext.DatabaseName);
                int timezoneId = currentCompany?.timezoneID ?? 0;

                var newCompany = await _companyService.CreateCompanyNewDBAsync(
                    registerModel.companyName,
                    registerModel.peNumber,
                    false,
                    accessContext.DatabaseName,
                    accessContext.CompanyId,
                    timezoneId);

                if (newCompany == null || newCompany.companyId <= 0)
                {
                    return new ApiResponse<RegisterNewCompanyResult>
                    {
                        Success = false,
                        Message = "Something went wrong. Try again creating a company.",
                        Data = null
                    };
                }

                await _companyService.InsertAdminCompanyAsync(newCompany.companyId, admin.adminID, true, accessContext.DatabaseName);

                Roles role = await _companyService.InsertAdminRoleAsync(accessContext.DatabaseName, newCompany.companyId, admin.email ?? ctx.Email);
                await _adminsService.AddAdminRole(admin.adminID, role.roleID, admin.email ?? ctx.Email, accessContext.DatabaseName, newCompany.companyId);

                department newDept = await _departmentService.CreateDepartmentAsync("default", "0001", accessContext.UserId, accessContext.DatabaseName);

                if (newDept != null && newDept.departmentID > 0)
                {
                    await _companyService.CreateCompanyDepartmentAsync(
                        newDept.departmentID,
                        newCompany.companyId,
                        admin.adminID.ToString(),
                        accessContext.DatabaseName);
                }

                return new ApiResponse<RegisterNewCompanyResult>
                {
                    Success = true,
                    Message = "Company is created successfully.",
                    Data = new RegisterNewCompanyResult
                    {
                        companyId = newCompany.companyId,
                        departmentId = newDept?.departmentID ?? 1,
                        adminId = admin.adminID,
                        companyName = newCompany.name,
                        message = "Company is created successfully."
                    }
                };
            }
            catch (Exception ex)
            {
                await LoggingHelper.InsertException(
                    ex.Source ?? string.Empty,
                    ex.Message,
                    "CompanyManager.RegisterNewCompanyAsync",
                    ex.StackTrace ?? string.Empty,
                    ex.InnerException?.ToString() ?? string.Empty);

                return new ApiResponse<RegisterNewCompanyResult>
                {
                    Success = false,
                    Message = "Something went wrong. Try again creating a company.",
                    Data = null
                };
            }
        }

        private async Task<UserAccessContext?> ResolveAccessContextAsync(CurrentUserContext ctx)
        {
            return await _userAccessContextManager.GetAsync(ctx);
        }

        private static bool IsValidRegisterRequest(registerCompany registerModel)
        {
            if (registerModel == null)
                return false;

            return
                !string.IsNullOrWhiteSpace(registerModel.firstName) &&
                !string.IsNullOrWhiteSpace(registerModel.lastName) &&
                !string.IsNullOrWhiteSpace(registerModel.phoneNumber) &&
                !string.IsNullOrWhiteSpace(registerModel.adminEmail) &&
                !string.IsNullOrWhiteSpace(registerModel.peNumber) &&
                !string.IsNullOrWhiteSpace(registerModel.companyName) &&
                registerModel.countryID > 0 &&
                registerModel.industryID > 0 &&
                !string.IsNullOrWhiteSpace(registerModel.description) &&
                !string.IsNullOrWhiteSpace(registerModel.companyURL);
        }

        private static string BuildDatabaseName(string companyName)
        {
            string dbName = (companyName ?? string.Empty).ToLowerInvariant().Trim().Replace(" ", "");
            dbName = Regex.Replace(dbName, "[^a-zA-Z0-9]", string.Empty);

            if (dbName.Length > 15)
                dbName = dbName.Substring(0, 15);

            string randomVal = Regex.Replace(Guid.NewGuid().ToString(), "[^a-zA-Z0-9]", string.Empty);
            return $"{dbName}.{randomVal.Substring(0, 6)}";
        }

    }
}