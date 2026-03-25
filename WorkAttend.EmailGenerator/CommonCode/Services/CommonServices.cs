using WorkAttend.API.Gateway.DAL.GeneralServices;
using WorkAttend.API.Gateway.DAL.genericRepository;
using WorkAttend.API.Gateway.DAL.services;
using WorkAttend.API.Gateway.DAL.services.SellServices;
using WorkAttend.EmailGenerator.CommonCode.Services;
using WorkAttend.EmailGenerator.Models;
using WorkAttend.Model;
using WorkAttend.Model.Enums;
using WorkAttend.Model.Models;
using WorkAttend.Shared.DataServices;
using WorkAttend.Shared.Enums;
using WorkAttend.Shared.Extensions;
using WorkAttend.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WorkAttend.EmailGenerator.CommonCode.Helpers
{
    public class CommonServices: ICommonServicesEmailGenerator
    {
        private readonly ISetupServices _SetupServices;
        private readonly ISellServices _SellServices;
        public CommonServices(ISetupServices setupServices, ISellServices sellServices)
        {
            _SetupServices = setupServices;
            _SellServices = sellServices;
        }
        public RegisterUserModel getVerificationInfo(int id)
        {
            RegisterUserModel model = new RegisterUserModel();
            EmailToken email = null;

            StoreRequest store = null;

            try
            {
                using (WorkAttendRepository context = DataContextHelper.GetPOSPortalContext())
                {
                    var Esql = PetaPoco.Sql.Builder.Select("*").From("EmailTokens").Where("EmailTokenID = @0", id);
                    email = context.Fetch<EmailToken>(Esql).FirstOrDefault();

                    var Ssql = PetaPoco.Sql.Builder.Select("*").From("StoreRequests").Where("StoreRequestID = @0", email != null ? email.RowID ?? 0 : 0);
                    store = context.Fetch<StoreRequest>(Ssql).FirstOrDefault();

                    //store = context.SingleOrDefault<StoreRequest>(email.RowID);
                    //email = context.SingleOrDefault<EmailToken>(email.EmailTokenID);

                    model.WebsiteURL = GlobalAppConfigs.EmailVerificationURL;
                    model.Name = store.FirstName + " " + store.LastName;
                    model.EmailAddress = store.Email;
                    model.VerifyEmailLink = model.WebsiteURL + store.StoreEmailVerficiationGuid.ToString();
                    model.PhoneNo = store.Phone;
                    model.StoreName = store.StoreName;
                    model.ActivationURL = "Test";
                }
            }
            catch (Exception ex)
            {
                throw (new Exception("Unable to send this email."));
            }

            return model;
        }

        public SellDetailModel getSaleInfo(int id, string dbName)
        {
            SellDetailModel model = new SellDetailModel();
            EmailToken email = null;
            SalesOrder sales = null;
            try
            {
                using (WorkAttendRepository context = DataContextHelper.GetPOSPortalContext())
                {
                    var Esql = PetaPoco.Sql.Builder.Select("*").From("EmailTokens").Where("EmailTokenID = @0", id);
                    email = context.Fetch<EmailToken>(Esql).FirstOrDefault();
                }

                if (email != null)
                {
                    using var storeContext = DataContextHelper.GetStoreDataContext(dbName);
                    var Ssql = PetaPoco.Sql.Builder.Select("*").From("salesorder").Where("SalesOrderID = @0", email.RowID);
                    sales = storeContext.Fetch<SalesOrder>(Ssql).FirstOrDefault();

                    if (sales != null)
                    {
                        //GetHistoryModel info = new GetHistoryModel();
                        //info.SalesOrderID = "139&&_ASDASD_ASDASD_ASDASD_ASD";//sales.SalesOrderID.EncryptID();
                        //info.OutID = sales.OutletID.EncryptID();
                        //var storeInfo = GeneralMethod.SelectOneRowByCondition<SellDetailModel>("Stores", "StoreName, CurrencyTypeID", "isActive=1", false, dbName);

                        var GetCurrencyTypeId = GeneralMethod.SelectOneRowByCondition<string>("Stores", "CurrencyTypeID", "isActive=1", false, dbName);

                        var taxInclusive = GeneralMethod.SelectOneRowByCondition<StoreConfiguration>("StoreConfigurations", "`Key`=@0 and isActive=true", false, dbName, GeneralConfigurations.TaxExclusive.ToString());
                        var isLoyaltyActive = GeneralMethod.SelectOneRowByCondition<StoreConfiguration>("StoreConfigurations", "`Key`=@0 and isActive=true", false, dbName, GeneralConfigurations.Loyalty.ToString());
                        model.IsLoyaltyActive = isLoyaltyActive != null && isLoyaltyActive.Value == "True" ? true : false;

                        var CurrencyData = GeneralMethod.SelectOneRowByConditionQueryable<CurrencyType>("CurrencyTypes", "CurrencyTypeID=@0 and IsActive=1", true, null, Convert.ToInt32(GetCurrencyTypeId))
                                .Select(x => new CommonModel.CurrencyData
                                {
                                    CurrencyName = x.CurrencyName,
                                    CurrencyCode = x.CurrencyCode,
                                    SCurrencyTypeId = x.CurrencyTypeID.EncryptID(),
                                    CurrencySymbol = x.CurrencySymbol
                                }).SingleOrDefault();
                        //model = SellServices.Instance.GetHistorySpecificDetailService(dbName, info);
                        SaleHistorySearchModel searchModel = new SaleHistorySearchModel()
                        {
                            OutletID = sales.OutletID.EncryptID(),
                            SalesOrderID = sales.SalesOrderID.EncryptID(),
                            CurrentPageNo = 1,
                            PageSize = 10,
                            //IsRefund = sales.isre,
                        };


                        List<SellDetailModel> saleInfo = _SellServices.GetSalesHistoryByIDService(dbName, searchModel);
                        model = saleInfo != null && saleInfo.Any() ? saleInfo.FirstOrDefault() : new SellDetailModel();
                        EditTemplateRequest info = new EditTemplateRequest();
                        info.TemplateID = ((int)ReceiptTemplatesEnums.StandardReceipt).EncryptID();//StandardTemplate , Need to make Enums

                        model.ReceiptTemplate = _SetupServices.GetTemplatesService(info, dbName).FirstOrDefault();
                        string url = _SetupServices.GetUploadedFileURL(dbName, info.TemplateID.DecryptID(), (int)StoreEntitiesEnum.ReceiptTemplates);
                        model.ReceiptTemplate.HeadImageURL = string.Format("{0}/{1}", GlobalAppConfigs.BaseResourceURL, url);
                        model.IsExclusive = taxInclusive != null && taxInclusive.Value == "True" ? true : false;
                        if (model.TaxesSum != null)
                        {
                            foreach (var tax in model.TaxesSum)
                            {
                                if (model.DiscountIsPercentage && model.DiscountPercentage > 0)
                                {
                                    tax.TaxPrice -= (tax.TaxPrice * (model.DiscountPercentage / 100));
                                }
                            }
                        }
                        if (model.IsExclusive)
                        {
                            foreach (var prod in model.Products)
                            {
                                prod.TotalPrice = prod.UnitPrice;
                            }
                        }
                        model.currencyData = CurrencyData;
                        //model.StoreName = storeInfo.StoreName;
                        //model.OutletName = sales != null && sales.OutletID > 0 ? GeneralMethod.SelectOneRowByCondition<string>("storeoutlets", "storeoutletid = @0", false, dbName, sales.OutletID) : string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (new Exception("Unable to send this email."));
            }

            return model;
        }

        private decimal CalculateDiscountPercentage(bool isExclusive, decimal price, decimal tax, decimal discount)
        {
            decimal discountPercent = 0;
            if (price > 0)
            {
                if (isExclusive)
                {
                    discountPercent = (discount / price) * 100;
                }
                else
                {
                    price += (price * tax);
                    discountPercent = (discount / price) * 100;
                }

                if (discountPercent < 0)
                {
                    discountPercent *= -1;
                }
            }
            return discountPercent;
        }

        public ContactUsModel ContactUsService(int id)
        {
            ContactUsModel model = new ContactUsModel();
            ContactInfo info = new ContactInfo();
            try
            {
                using (WorkAttendRepository context = DataContextHelper.GetPOSPortalContext())
                {
                    var Esql = PetaPoco.Sql.Builder.Select("*").From("contactinfo").Where("ContactInfoID = @0", id);
                    info = context.Fetch<ContactInfo>(Esql).FirstOrDefault();
                    if (info != null)
                    {
                        var country = context.Fetch<Country>("select * from countries where countryid = @0", info.CountryID).FirstOrDefault();
                        var business = context.Fetch<TypeOfBusiness>("select * from typeofbusinesses where BusinessTypeID = @0", info.BusinessID).FirstOrDefault();

                        model.BusinessName = info.BusinessName;
                        model.Country = country.CountryName;
                        model.EmailAddress = info.Email;
                        model.PhoneNo = info.PhoneNo;
                        model.BusinessType = business.BusinessTypeName.ToUpper();
                        model.Locations = info.NumberOfLocation;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (new Exception("Unable to send this email."));
            }

            return model;
        }

        public SubPaymentModel PaymentSubscriptionService(int id)
        {
            SubPaymentModel model = new SubPaymentModel();
            StoreRequest info = new StoreRequest();
            try
            {
                using (WorkAttendRepository context = DataContextHelper.GetPOSPortalContext())
                {
                    var Esql = PetaPoco.Sql.Builder.Select("*").From("storerequests").Where("StoreRequestID = @0", id);
                    info = context.Fetch<StoreRequest>(Esql).FirstOrDefault();
                    if (info != null)
                    {
                        var ssql = PetaPoco.Sql.Builder.Select("*").From("storesubscriptionpackages").Where("StoreRequestID = @0 and isActive = true", id);
                        var storePkgInfo = context.Fetch<StoreSubscriptionPackage>(ssql).FirstOrDefault();
                        var curr = context.Fetch<CurrencyType>("select currencycode from WorkAttenddb.currencytypes where CurrencyTypeID = @0", info.CurrencyTypeID).FirstOrDefault();
                        if (storePkgInfo != null)
                        {
                            var spsql = PetaPoco.Sql.Builder.Select("*").From("subscriptionpackages").Where("SubscriptionPackageID = @0", storePkgInfo.SubscriptionPackageID);
                            var pkgInfo = context.Fetch<SubscriptionPackage>(spsql).FirstOrDefault();
                            model.UserName = info.FirstName + " " + info.LastName;
                            model.ContactUs = GlobalAppConfigs.ContactUsEmail;
                            model.LoginURL = GlobalAppConfigs.LoginURL.Replace("app", info.StoreURL);
                            model.PlanPrice = storePkgInfo.TotalAmountPaid.HasValue ? storePkgInfo.TotalAmountPaid.Value.ToString() : string.Empty;
                            model.PlanName = pkgInfo.SubName;
                            model.NextPaymentDate = storePkgInfo.SubEndDate.ToString("d MMMM, yyyy h:mm tt");
                            model.NbrOfOutlet = storePkgInfo.NoOfOutlet;
                            model.NbrOfRegister = storePkgInfo.NoOfRegister;
                            model.StoreName = info.StoreName;
                            model.Currency = curr != null && !string.IsNullOrEmpty(curr.CurrencyCode) ? curr.CurrencyCode : string.Empty;
                            model.HasAnalytics = storePkgInfo.HasAnalytics == true ? "ON" : "OFF";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw (new Exception("Unable to send this email."));
            }

            return model;
        }

        public ForgetPassModel ForgetPassEmailService(int id)
        {
            ForgetPassModel model = new ForgetPassModel();
            Users info = new Users();
            EmailToken emailToken = new EmailToken();
            try
            {
                using (WorkAttendRepository context = DataContextHelper.GetPOSPortalContext())
                {
                    var Esql = PetaPoco.Sql.Builder.Select("*").From("EmailTokens").Where("EmailTokenID = @0", id);
                    emailToken = context.Fetch<EmailToken>(Esql).FirstOrDefault();
                }

                if (emailToken != null)
                {
                    using var storeContext = DataContextHelper.GetStoreDataContext(emailToken.StoreURL);
                    var Ssql = PetaPoco.Sql.Builder.Select("*").From("users").Where("userid = @0", emailToken.RowID);
                    info = storeContext.Fetch<Users>(Ssql).FirstOrDefault();

                    if (info != null)
                    {
                        model.PassCode = info.UserPassword.Decrypt();
                        model.FirstName = info.UserFirstName;
                        model.LastName = info.UserLastName;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (new Exception("Unable to send this email."));
            }

            return model;
        }
        public AccReminderPassModel OnAccountReminderService(int id)
        {
            AccReminderPassModel model = new AccReminderPassModel();
            CustomerProfileTable info = new CustomerProfileTable();
            EmailToken emailToken = new EmailToken();
            try
            {
                using (WorkAttendRepository context = DataContextHelper.GetPOSPortalContext())
                {
                    var Esql = PetaPoco.Sql.Builder.Select("*").From("EmailTokens").Where("EmailTokenID = @0", id);
                    emailToken = context.Fetch<EmailToken>(Esql).FirstOrDefault();
                }

                if (emailToken != null)
                {
                    using var storeContext = DataContextHelper.GetStoreDataContext(emailToken.StoreURL);
                    var Ssql = PetaPoco.Sql.Builder.Select("*").From("customerprofiletable").Where("CustomerProfileId = @0", emailToken.RowID);
                    info = storeContext.Fetch<CustomerProfileTable>(Ssql).FirstOrDefault();

                    if (info != null)
                    {

                        var store_name = GeneralMethod.SelectOneRowByCondition<string>("Stores", "storename", "isActive=1", false, emailToken.StoreURL);
                        model.StoreName = !string.IsNullOrEmpty(store_name) ? store_name : string.Empty;
                        model.CustomerCode = info.CustomerCode;
                        model.FirstName = info.FirstName;
                        model.LastName = info.LastName;
                        model.TotalAccount = info.TotalAccount;
                        model.AccountCredit = info.AccountCredit;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (new Exception("Unable to send this email."));
            }

            return model;
        }

    }
}
