using System.Threading.Tasks;
using DevMeetings.Services.Notifications.Api.Infrastructure.Dtos;

namespace DevMeetings.Services.Notifications.Api.Infrastructure.Persistence.Repositories
{
    public interface IMailRepository
    {
        Task<EmailTemplateDto> GetTemplate(string @event);
    }
}