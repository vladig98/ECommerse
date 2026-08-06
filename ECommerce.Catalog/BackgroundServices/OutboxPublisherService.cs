namespace ECommerce.Catalog.BackgroundServices;

public class OutboxPublisherService(
    IServiceProvider serviceProvider,
    IMessageProducer messageProducer,
    ILogger logger) : BackgroundService
{
    private const string topic = "products";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Information("Starting Outbox Publisher...");

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

                foreach (EventMessage msg in messages)
                {
                    IntegrationEvent integrationEvent = new(msg.Id, msg.Key, msg.EventType, msg.Value);
                    await messageProducer.PublishAsync(topic, integrationEvent, stoppingToken);
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