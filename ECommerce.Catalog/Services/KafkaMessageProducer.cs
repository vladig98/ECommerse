namespace ECommerce.Catalog.Services;

public class KafkaMessageProducer(IProducer<string, ISpecificRecord> producer) : IMessageProducer, IDisposable
{
    private readonly IProducer<string, ISpecificRecord> _producer = producer;

    public async Task PublishAsync<T>(string topic, string key, T message, CancellationToken token) where T : ISpecificRecord
    {
        Message<string, ISpecificRecord> kafkaMessage = new()
        {
            Key = key,
            Value = message
        };

        await _producer.ProduceAsync(topic, kafkaMessage, token);
    }

    public void Dispose()
    {
        _producer.Dispose();
        GC.SuppressFinalize(this);
    }
}