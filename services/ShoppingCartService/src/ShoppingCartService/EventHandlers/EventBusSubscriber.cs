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
            _channel.QueueDeclare(queue: GlobalConstants.RabbitMQProductCreatedEventName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var userCreatedConsumer = new EventingBasicConsumer(_channel);
            userCreatedConsumer.Received += async (model, ea) =>
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

            var productUpdatedConsumer = new EventingBasicConsumer(_channel);
            productUpdatedConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var productUpdatedEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(message);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();
                    await cartService.HandleProductCreatedEvent(productUpdatedEvent!);
                }
            };

            _channel.BasicConsume(queue: GlobalConstants.RabbitMQUserCreatedEventName, autoAck: true, consumer: userCreatedConsumer);
            _channel.BasicConsume(queue: GlobalConstants.RabbitMQProductCreatedEventName, autoAck: true, consumer: productUpdatedConsumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
