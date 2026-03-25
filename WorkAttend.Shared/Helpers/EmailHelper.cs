using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Globalization;
using System.Net.Mail;
using System.Threading.Tasks;

namespace WorkAttend.Shared.Helpers
{
    public static class EmailHelper
    {
        public static async Task<bool> SendRegisterEmail(
            IConfiguration configuration,
            string toEmail,
            string toName,
            string companyName,
            string password,
            string? loginUrl = null)
        {
            try
            {
                var apiKey =
                    configuration["AppSettings:SGApiKey"] ??
                    configuration["SGApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                    return false;

                var companyBaseUrl =
                    configuration["AppSettings:companyBaseURL"] ??
                    configuration["companyBaseURL"] ??
                    string.Empty;

                var resolvedLoginUrl = !string.IsNullOrWhiteSpace(loginUrl)
                    ? loginUrl
                    : companyBaseUrl;

                var displayName = new CultureInfo("en-US").TextInfo.ToTitleCase(toName ?? string.Empty);

                string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Welcome to WorkAttend</h2>
                        <p>Hello {displayName},</p>
                        <p>Your admin account has been created successfully.</p>
                        <p><strong>Company:</strong> {companyName}</p>
                        <p><strong>Email:</strong> {toEmail}</p>
                        <p><strong>Password:</strong> {password}</p>
                        <p><strong>Login URL:</strong> {resolvedLoginUrl}</p>
                    </body>
                    </html>";

                var fromAddress = new EmailAddress("no-reply@workattend.com", "Dakar Software Systems");
                var toAddress = new EmailAddress(toEmail, displayName);

                var client = new SendGridClient(apiKey);
                var msg = MailHelper.CreateSingleEmail(fromAddress, toAddress, "Welcome to WorkAttend", string.Empty, body);

                var response = await client.SendEmailAsync(msg);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
