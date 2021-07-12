using System;

namespace DevMeetings.Services.Notifications.Api.Infrastructure.Dtos
{
    public class EmailTemplateDto
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Event { get; set; }
    }
}