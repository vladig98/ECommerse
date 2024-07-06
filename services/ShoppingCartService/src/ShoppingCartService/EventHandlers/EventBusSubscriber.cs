using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ShoppingCartService.EventHandlers
{
    public class EventBusSubscriber : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public EventBusSubscriber(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory() { HostName = GlobalConstants.RabbitMQHostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: GlobalConstants.RabbitMQUserCreatedEventName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
                    await cartService.HandleUserCreatedEvent(userCreatedEvent!);
                }
            };

            _channel.BasicConsume(queue: GlobalConstants.RabbitMQUserCreatedEventName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
