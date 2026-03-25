using dakarPOS.API.Gateway.BLL.InterfaceBLL;
using dakarPOS.API.Gateway.BLL.ServicesBLL; 
using dakarPOS.API.Gateway.DAL.services;
using dakarPOS.API.Gateway.DAL.services.CommonService;
using dakarPOS.API.Gateway.DAL.services.CommonServiceDependencies;
using dakarPOS.API.Gateway.DAL.services.CommonServices;
using dakarPOS.API.Gateway.DAL.services.InventoryServices;
using dakarPOS.API.Gateway.DAL.services.PromotionServices;
using dakarPOS.API.Gateway.DAL.services.RolesServices;
using dakarPOS.API.Gateway.DAL.services.storeServices;
using dakarPOS.API.Gateway.DAL.services.userServices;
using Microsoft.Extensions.DependencyInjection;
using System;
namespace InventoryExecutionProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IStorePackageInfoProvider, StorePackageInfoProvider>();
            services.AddSingleton<IStoreServices, StoreServices>();
            services.AddSingleton<IPromotionAndCommonServiceMethods, PromotionAndCommonServiceMethods>();
            services.AddSingleton<IPromotionServices, PromotionServices>();
            services.AddSingleton<ISellAndCommonServicesMethods, SellAndCommonServicesMethods>();
            services.AddSingleton<IRolesServices , RolesServices>();
            services.AddSingleton<ISetupServices, SetupServices>();
            services.AddSingleton<IInventoryServices, InventoryServices>();
            services.AddSingleton<ICommonServices, CommonService>();
            services.AddSingleton<IUserVerificationService , UserVerificationService>();
            services.AddSingleton<ICommonServicesBLL , CommonServicesBLL>();
            services.AddSingleton<IInventoryServices, InventoryServices>();

            services.AddSingleton<IInventoryServicesBLL, InventoryServicesBLL>();



            var provider = services.BuildServiceProvider();

            var inventoryBLL = provider.GetRequiredService<IInventoryServicesBLL>();
            var runner = new InventoryExecutionClass(inventoryBLL);

            runner.InventoryExecutionProcess();
        }
    }



}
