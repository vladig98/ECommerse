namespace ECommerce.Catalog.Services;

public class KafkaMessageProducer(IProducer<string, string> producer) : IMessageProducer, IDisposable
{
    private readonly IProducer<string, string> _producer = producer;

    public async Task PublishAsync(string topic, IntegrationEvent message, CancellationToken token)
    {
        Message<string, string> kafkaMessage = new()
        {
            Key = message.Key,
            Value = message.Payload,
            Headers = new Headers
            {
                { "eventType", Encoding.UTF8.GetBytes(message.EventType) },
                { "eventId", Encoding.UTF8.GetBytes(message.EventId.ToString()) }
            }
        };

        await _producer.ProduceAsync(topic, kafkaMessage, token);
    }

    public void Dispose()
    {
        _producer.Dispose();
        GC.SuppressFinalize(this);
    }
}