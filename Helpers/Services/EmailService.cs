using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using SendGrid;

namespace Hiper.Api.Helpers.Services
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return configSendGridasync(message);
        }

        public Task SendAsync(SendGridMessage message)
        {
            return configSendGridasync(message);
        }


        private Task configSendGridasync(IdentityMessage message)
        {
            var myMessage = new SendGridMessage();
            myMessage.AddTo(message.Destination);
            myMessage.From = new MailAddress(
                ConfigurationManager.AppSettings["mailSenderAddress"], "Hiper team");
            myMessage.Subject = message.Subject;
            myMessage.Text = message.Body;
            myMessage.Html = message.Body;

            var credentials = new NetworkCredential(
                ConfigurationManager.AppSettings["mailAccount"],
                ConfigurationManager.AppSettings["mailPassword"]
                );

            // Create a Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.

            return transportWeb.DeliverAsync(myMessage);
        }

        private Task configSendGridasync(SendGridMessage message)
        {
            var myMessage = message;

            myMessage.From = new MailAddress(
                ConfigurationManager.AppSettings["mailSenderAddress"], "Hiper team");
            myMessage.Subject = message.Subject;
            myMessage.Text = message.Text;
            myMessage.Html = message.Html;

            var credentials = new NetworkCredential(
                ConfigurationManager.AppSettings["mailAccount"],
                ConfigurationManager.AppSettings["mailPassword"]
                );

            // Create a Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.

            return transportWeb.DeliverAsync(myMessage);
        }
    }
}