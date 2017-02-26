using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public AuthMessageSender(IOptions<AuthMessageSMSSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSMSSenderOptions Options { get; }  // set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }

        public Task SendSmsAsync(string number, string message)
        {
            var SID = "AC02a0015664dc24eaa87a6858bbd83407";
            var AuthToken = "beb828b769d64666169a4e8c7ba4d0ca";
            var twilio = new Twilio.TwilioRestClient(
                SID,           // Account Sid from dashboard
                AuthToken);    // Auth Token


            var result = twilio.SendMessage("+14782922479", "+14784644779", message);
            // Use the debug output for testing without receiving a SMS message.
            // Remove the Debug.WriteLine(message) line after debugging.
            // System.Diagnostics.Debug.WriteLine(result);
            return Task.FromResult(0);
        }
    }
}
