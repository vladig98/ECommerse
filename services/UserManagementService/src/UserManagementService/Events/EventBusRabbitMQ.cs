using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace UserManagementService.Events
{
    public class EventBusRabbitMQ : IEventBus
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public EventBusRabbitMQ()
        {
            var factory = new ConnectionFactory() { HostName = GlobalConstants.RabbitMQHostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Publish<T>(T @event) where T : class
        {
            var eventName = typeof(T).Name;

            _channel.QueueDeclare(queue: eventName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: string.Empty, routingKey: eventName, basicProperties: null, body: body);
        }
    }
}
