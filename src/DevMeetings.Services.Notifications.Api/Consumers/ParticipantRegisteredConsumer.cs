using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevMeetings.Services.Notifications.Api.Infrastructure.Services.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DevMeetings.Services.Notifications.Api.Consumers
{
    public class ParticipantRegisteredConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string Queue = "notification-service/participant-registered";
        private const string Exchange = "notification-service";

        public ParticipantRegisteredConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;

            var connectionFactory = new ConnectionFactory {
                HostName = "localhost"
            };

            _connection = connectionFactory.CreateConnection("notifications-service-participant-registered-consumer"); 

            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(Exchange, "topic", true);
            _channel.QueueDeclare(Queue, false, false, false, null);
            _channel.QueueBind(Queue, Exchange, Queue);

            _channel.QueueBind(Queue, "meeting-service", "participant-registered");
        }

        private async Task<bool> SendEmail(ParticipantRegistered participant) {
            using (var scope = _serviceProvider.CreateScope()) {
                var emailService = scope.ServiceProvider.GetService<INotificationService>();

                var content = string.Format("Participation confirmed!", participant.FullName);

                await emailService.SendAsync("You are confirmed!", content, participant.Email, participant.FullName);

                return true;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) => {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<ParticipantRegistered>(contentString);

                Console.WriteLine($"Message ParticipantRegistered received with Id {message.Id}");

                await SendEmail(message);

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(Queue, false, consumer);
                        
            return Task.CompletedTask;
        }
    }

    public class ParticipantRegistered
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}