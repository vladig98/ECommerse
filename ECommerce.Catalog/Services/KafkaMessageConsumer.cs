namespace ECommerce.Catalog.Services;

public class KafkaMessageConsumer(IConsumer<Ignore, ISpecificRecord> consumer, ILogger logger) : IMessageConsumer
{
    private readonly IConsumer<Ignore, ISpecificRecord> _consumer = consumer;
    private readonly ILogger _logger = logger;
    private ConsumeResult<Ignore, ISpecificRecord>? _lastResult;

    public void Subscribe(string topic)
    {
        _consumer.Subscribe(topic);
    }

    public IntegrationEvent? Consume(CancellationToken token)
    {
        try
        {
            _lastResult = _consumer.Consume(token);
            if (_lastResult is not { Message.Value: not null })
            {
                return null;
            }

            ISpecificRecord avroMessage = _lastResult.Message.Value;
            Guid eventId = Guid.NewGuid();

            if (_lastResult.Message.Headers?.TryGetLastBytes("eventId", out byte[]? idBytes) == true)
            {
                if (!Guid.TryParse(Encoding.UTF8.GetString(idBytes), out Guid parsedId))
                {
                    _logger.Warning("Message received with an invalid 'eventId' format.");
                }
                else
                {
                    eventId = parsedId;
                }
            }

            string eventType = avroMessage.Schema.Name;

            return new IntegrationEvent(eventId, string.Empty, eventType, avroMessage);
        }
        catch (ConsumeException ex)
        {
            _logger.Error(ex, "Kafka consume error or Schema Registry deserialization failure.");
            return null;
        }
    }

    public void Commit()
    {
        if (_lastResult is not null)
        {
            _consumer.Commit(_lastResult);
            _lastResult = null;
        }
    }

    public void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        GC.SuppressFinalize(this);
    }
}