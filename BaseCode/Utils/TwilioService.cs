using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BaseCode.Utils
{
    public static class TwilioService
    {
        private static readonly string AccountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        private static readonly string AuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        private static readonly string FromNumber = Environment.GetEnvironmentVariable("TWILIO_FROM_NUMBER");

        public static bool SendSms(string toNumber, string message)
        {
            try
            {
                TwilioClient.Init(AccountSid, AuthToken);
                var messageOptions = new CreateMessageOptions(new PhoneNumber(toNumber))
                {
                    From = new PhoneNumber(FromNumber),
                    Body = message
                };
                var twilioMessage = MessageResource.Create(messageOptions);

                return !string.IsNullOrEmpty(twilioMessage.Sid);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
