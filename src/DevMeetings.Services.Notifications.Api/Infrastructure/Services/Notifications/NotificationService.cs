using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DevMeetings.Services.Notifications.Api.Infrastructure.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly MailConfig _config;
        private readonly ISendGridClient _sendGridClient;
        public NotificationService(ISendGridClient sendGridClient, MailConfig config)
        {
            _config = config;
            _sendGridClient = sendGridClient;
        }

        public async Task SendAsync(string subject, string content, string toEmail, string toName){
            var from = new EmailAddress(_config.FromEmail, _config.FromName);
            var to = new EmailAddress(toEmail, toName);

            var message = new SendGridMessage {
                From = from,
                Subject = subject
            };

            message.AddContent(MimeType.Html, content);
            message.AddTo(to);

            message.SetClickTracking(false, false);
            message.SetOpenTracking(false);
            message.SetGoogleAnalytics(false);
            message.SetSubscriptionTracking(false);
            
            await _sendGridClient.SendEmailAsync(message);
        }
    }
}