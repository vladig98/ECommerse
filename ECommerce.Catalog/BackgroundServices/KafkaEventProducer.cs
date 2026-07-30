namespace ECommerce.Catalog.BackgroundServices;

public class KafkaEventProducer(
    IOptions<KafkaSettings> options,
    IServiceProvider serviceProvider,
    ILogger logger) : BackgroundService
{
    private const string topic = "products";
    private readonly ProducerConfig config = new()
    {
        BootstrapServers = options.Value.Server,
        SaslUsername = options.Value.Username,
        SaslPassword = options.Value.Password,
        SecurityProtocol = SecurityProtocol.SaslSsl,
        SaslMechanism = SaslMechanism.Plain,
        Acks = Acks.All
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Information("Starting Kafka Outbox Publisher...");
        using IProducer<string, string> producer = new ProducerBuilder<string, string>(config).Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
                MainDbContext dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();

                List<EventMessage> messages = await dbContext.EventMessages
                    .OrderBy(x => x.CreatedAt)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                if (messages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                foreach (EventMessage eventMessage in messages)
                {
                    Message<string, string> message = new()
                    {
                        Key = eventMessage.Key,
                        Value = eventMessage.Value,
                        Headers = new Headers
                        {
                            { "eventType", Encoding.UTF8.GetBytes(eventMessage.EventType) }
                        }
                    };

                    await producer.ProduceAsync(topic, message, stoppingToken);
                }

                dbContext.EventMessages.RemoveRange(messages);
                await dbContext.SaveChangesAsync(stoppingToken);

                logger.Debug("Published and cleared {Count} outbox messages.", messages.Count);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to process outbox messages. Retrying in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
