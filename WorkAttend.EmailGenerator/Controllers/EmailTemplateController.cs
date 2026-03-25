using WorkAttend.EmailGenerator.CommonCode.Helpers;
using WorkAttend.EmailGenerator.CommonCode.Services;
using WorkAttend.EmailGenerator.Models;
using WorkAttend.Model;
using WorkAttend.Shared.DataServices;
using WorkAttend.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace WorkAttend.EmailGenerator.Controllers
{
    public class EmailTemplateController : Controller
    {
        private readonly ILogger<EmailTemplateController> _logger;
        private readonly ICommonServicesEmailGenerator _commonServicesEmailGenerator;

        public EmailTemplateController(ILogger<EmailTemplateController> logger , ICommonServicesEmailGenerator commonServicesEmailGenerator)
        {
            _logger = logger;
            _commonServicesEmailGenerator = commonServicesEmailGenerator;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public ActionResult RegisterStore(int id /*email token id*/)
        {
            //ELHelper elh = new ELHelper();
            RegisterUserModel model = new RegisterUserModel();
            model = _commonServicesEmailGenerator.getVerificationInfo(id);
            return (View(model));
        }

        public ActionResult SaleEmailReceipt(int id /*email token id*/, string dbName)
        {
            //ELHelper elh = new ELHelper();
            SellDetailModel model = _commonServicesEmailGenerator.getSaleInfo(id, dbName);
            return (View(model));
        }

        public ActionResult StoreWelcome(int id /*email token id*/)
        {
            RegisterUserModel model = new RegisterUserModel();
            model = _commonServicesEmailGenerator.getVerificationInfo(id);
            return (View(model));
        }

        public ActionResult ContactUs(int id /*row id which pk of contact us*/)
        {
            ContactUsModel model = new ContactUsModel();
            model = _commonServicesEmailGenerator.ContactUsService(id);
            return (View(model));
        }

        public ActionResult PaymentSubscription(int id /*row id which pk of contact us*/)
        {
            SubPaymentModel model = new SubPaymentModel();
            model = _commonServicesEmailGenerator.PaymentSubscriptionService(id);
            return (View(model));
        }

        public ActionResult ForgetPassEmail(int id /*email token id*/)
        {
            ForgetPassModel model = new ForgetPassModel();
            model = _commonServicesEmailGenerator.ForgetPassEmailService(id);
            return (View(model));
        }

        public ActionResult OnAccountReminder(int id /*email token id*/)
        {
            AccReminderPassModel model = new AccReminderPassModel();
            model = _commonServicesEmailGenerator.OnAccountReminderService(id);
            return (View(model));
        }
    }
}
