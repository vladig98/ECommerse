namespace ECommerce.Catalog.Services;

internal class KafkaMessageConsumer(IConsumer<Ignore, string> consumer, ILogger logger) : IMessageConsumer
{
    private readonly IConsumer<Ignore, string> _consumer = consumer;
    private readonly ILogger _logger = logger;
    private ConsumeResult<Ignore, string>? _lastResult;

    public void Subscribe(string topic)
    {
        _consumer.Subscribe(topic);
    }

    public IntegrationEvent? Consume(CancellationToken token)
    {
        try
        {
            _lastResult = _consumer.Consume(token);
            if (_lastResult?.Message is null)
            {
                return null;
            }

            string eventType = string.Empty;
            Guid eventId = Guid.NewGuid();

            if (_lastResult.Message.Headers?.TryGetLastBytes("eventType", out byte[]? typeBytes) == true)
            {
                eventType = Encoding.UTF8.GetString(typeBytes);
            }

            if (_lastResult.Message.Headers?.TryGetLastBytes("eventId", out byte[]? idBytes) == true)
            {
                if (!Guid.TryParse(Encoding.UTF8.GetString(idBytes), out Guid parsedId))
                {
                    _logger.Warning("Message received with an invalid 'eventId' format. Idempotency cannot be guaranteed.");
                }
                else
                {
                    eventId = parsedId;
                }
            }
            else
            {
                _logger.Warning("Message received without an 'eventId' header. Idempotency cannot be guaranteed.");
            }

            return new IntegrationEvent(eventId, string.Empty, eventType, _lastResult.Message.Value);
        }
        catch (ConsumeException ex)
        {
            _logger.Error(ex, "Kafka consume error.");
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