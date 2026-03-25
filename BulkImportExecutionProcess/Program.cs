using BulkImportExecutionProcess;
using dakarPOS.API.Gateway.BLL.InterfaceBLL;
using dakarPOS.API.Gateway.BLL.ServicesBLL;
using dakarPOS.API.Gateway.DAL.services;
using dakarPOS.API.Gateway.DAL.services.CommonService;
using dakarPOS.API.Gateway.DAL.services.CommonServiceDependencies;
using dakarPOS.API.Gateway.DAL.services.CommonServices;
using dakarPOS.API.Gateway.DAL.services.CustomerServices;
using dakarPOS.API.Gateway.DAL.services.InventoryServices;
using dakarPOS.API.Gateway.DAL.services.PaymentServices;
using dakarPOS.API.Gateway.DAL.services.PriceBookServices;   
using dakarPOS.API.Gateway.DAL.services.ProductServices;
using dakarPOS.API.Gateway.DAL.services.PromotionServices;
using dakarPOS.API.Gateway.DAL.services.RolesServices;
using dakarPOS.API.Gateway.DAL.services.SellSalesLogsServices;
using dakarPOS.API.Gateway.DAL.services.SellServices;
using dakarPOS.API.Gateway.DAL.services.storeServices;
using dakarPOS.API.Gateway.DAL.services.userServices;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ImportUtilityProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var services = new ServiceCollection();

                // Register nested dependency first
                services.AddSingleton<IStorePackageInfoProvider, StorePackageInfoProvider>();
                services.AddSingleton<IPromotionAndCommonServiceMethods, PromotionAndCommonServiceMethods>();
                services.AddSingleton<IPromotionServices, PromotionServices>();
                services.AddSingleton<ISellAndCommonServicesMethods,SellAndCommonServicesMethods>();
                services.AddSingleton<IRolesServices, RolesServices>();
                services.AddSingleton<ISetupServices, SetupServices>();
                services.AddSingleton<IInventoryServices, InventoryServices>();
                services.AddSingleton<ICommonServices, CommonService>();
                services.AddSingleton<ISellSalesLogsServices, SellSalesLogsServices>();
                services.AddSingleton<IPaymentServices, PaymentServices>();
                services.AddSingleton<ISellServices, SellServices>();
                services.AddSingleton<IStorePackageInfoProvider, StorePackageInfoProvider>();
                services.AddSingleton<IStoreServices,StoreServices>();
                services.AddSingleton<IUserVerificationService, UserVerificationService>();
                services.AddSingleton<ICommonServicesBLL, CommonServicesBLL>();
                services.AddSingleton<ICustomerService, CustomerServices>();
                //  Register DAL services used inside BLLs
                services.AddSingleton<IProductServices, ProductServices>();
                services.AddSingleton<IPriceBookServices, PriceBookServices>(); 

                //  Register BLL
                services.AddSingleton<IProductServicesBLL, ProductServicesBLL>();
                services.AddSingleton<IPriceBookServicesBLL, PriceBookServiceBLL>();
                services.AddSingleton<ICustomerServicesBLL, CustomerServicesBLL>();

                //  Runner Class 
                services.AddSingleton<BulkImportExecutionClass>();

                using var provider = services.BuildServiceProvider();

                var ie = provider.GetRequiredService<BulkImportExecutionClass>();
                ie.BulkImportExecutionProcess();

                Console.WriteLine("Product Imported Successfully");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Bulk Import Failed");
                Console.WriteLine("Product Imported Failed");
            }
        }
    }
}
