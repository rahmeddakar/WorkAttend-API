using WorkAttend.EmailGenerator.Models;
using WorkAttend.Model;

namespace WorkAttend.EmailGenerator.CommonCode.Services
{
    public interface ICommonServicesEmailGenerator
    {
        RegisterUserModel getVerificationInfo(int id);
        SellDetailModel getSaleInfo(int id, string dbName);
        ContactUsModel ContactUsService(int id);
        SubPaymentModel PaymentSubscriptionService(int id);
        ForgetPassModel ForgetPassEmailService(int id);
        AccReminderPassModel OnAccountReminderService(int id);
    }
}
