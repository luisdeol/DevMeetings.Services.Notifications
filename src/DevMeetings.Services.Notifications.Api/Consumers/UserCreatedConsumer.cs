using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevMeetings.Services.Notifications.Api.Infrastructure.Persistence.Repositories;
using DevMeetings.Services.Notifications.Api.Infrastructure.Services.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DevMeetings.Services.Notifications.Api.Consumers
{
    public class UserCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string Queue = "notification-service/user-created";
        private const string Exchange = "notification-service";
        public UserCreatedConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;

            var connectionFactory = new ConnectionFactory {
                HostName = "localhost"
            };

            _connection = connectionFactory.CreateConnection("notifications-service-consumer"); 

            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(Exchange, "topic", true);
            _channel.QueueDeclare(Queue, false, false, false, null);
            _channel.QueueBind(Queue, Exchange, Queue);

            _channel.QueueBind(Queue, "user-service", "user-created");
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) => {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<UserCreated>(contentString);

                Console.WriteLine($"Message UserCreated received with Id {message.Id}");

                await SendEmail(message);

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(Queue, false, consumer);
                        
            return Task.CompletedTask;
        }

        private async Task<bool> SendEmail(UserCreated user) {
            using (var scope = _serviceProvider.CreateScope()) {
                var mailRepository = scope.ServiceProvider.GetService<IMailRepository>();
                var emailService = scope.ServiceProvider.GetService<INotificationService>();

                var template = await mailRepository.GetTemplate("UserCreated");

                if (template == null) {
                    return false;
                }

                var content = string.Format(template.Content, user.FullName);

                await emailService.SendAsync(template.Subject, content, user.Email, user.FullName);

                return true;
            }
        }
    }

    public class UserCreated {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}