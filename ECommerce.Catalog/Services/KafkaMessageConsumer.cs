namespace ECommerce.Catalog.Services;

public class KafkaMessageConsumer : IMessageConsumer
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ILogger _logger;
    private ConsumeResult<Ignore, string>? _lastResult;

    public KafkaMessageConsumer(IOptions<KafkaSettings> options, ILogger logger)
    {
        _logger = logger;
        ConsumerConfig config = new()
        {
            BootstrapServers = options.Value.Server,
            SecurityProtocol = SecurityProtocol.Plaintext,
            GroupId = "catalog-service-inventory-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
    }

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

            if (_lastResult.Message.Headers.TryGetLastBytes("eventType", out byte[]? typeBytes))
            {
                eventType = Encoding.UTF8.GetString(typeBytes);
            }

            if (_lastResult.Message.Headers.TryGetLastBytes("eventId", out byte[]? idBytes))
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