using System.Threading.Tasks;

namespace DevMeetings.Services.Notifications.Api.Infrastructure.Services.Notifications
{
    public interface INotificationService
    {
        Task SendAsync(string subject, string content, string toEmail, string toName);
    }
}