using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace WorkAttend.Shared.Helpers
{
    public static class MessageHelper
    {
        public static async Task<bool> SendMessageToNumber(IConfiguration configuration, string text, string mobileNumber)
        {
            try
            {
                var accountSid =
                    configuration["AppSettings:TwilioAccountSid"] ??
                    configuration["TwilioAccountSid"];

                var authToken =
                    configuration["AppSettings:TwilioAuthToken"] ??
                    configuration["TwilioAuthToken"];

                var messagingServiceSid =
                    configuration["AppSettings:TwilioMessagingServiceSid"] ??
                    configuration["TwilioMessagingServiceSid"];

                if (string.IsNullOrWhiteSpace(accountSid) ||
                    string.IsNullOrWhiteSpace(authToken) ||
                    string.IsNullOrWhiteSpace(messagingServiceSid) ||
                    string.IsNullOrWhiteSpace(mobileNumber) ||
                    string.IsNullOrWhiteSpace(text))
                {
                    return false;
                }

                TwilioClient.Init(accountSid, authToken);

                var message = await MessageResource.CreateAsync(
                    body: text,
                    messagingServiceSid: messagingServiceSid,
                    to: new PhoneNumber(mobileNumber));

                return message != null;
            }
            catch
            {
                return false;
            }
        }
    }
}