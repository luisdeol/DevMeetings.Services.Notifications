using System;
using System.Threading.Tasks;
using DevMeetings.Services.Notifications.Api.Infrastructure.Dtos;
using MongoDB.Driver;

namespace DevMeetings.Services.Notifications.Api.Infrastructure.Persistence.Repositories
{
    public class MailRepository : IMailRepository
    {
        private readonly IMongoCollection<EmailTemplateDto> _collection;
        public MailRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<EmailTemplateDto>("email-templates");
        }
        public async Task<EmailTemplateDto> GetTemplate(string @event)
        {
            return await _collection.Find(c => c.Event == @event).SingleOrDefaultAsync();
        }
    }
}