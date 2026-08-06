namespace ECommerce.Catalog.BackgroundServices;

public class InventoryConsumerService(
    IServiceProvider serviceProvider,
    IMessageConsumer messageConsumer,
    ILogger logger) : BackgroundService
{
    private const string topic = "inventory";

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(
            () => RunConsumerLoop(stoppingToken),
            stoppingToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();
    }

    private async Task RunConsumerLoop(CancellationToken stoppingToken)
    {
        logger.Information("Starting Inventory Consumer...");
        messageConsumer.Subscribe(topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    IntegrationEvent? integrationEvent = messageConsumer.Consume(stoppingToken);

                    if (integrationEvent is not { EventType: nameof(InventoryLevelChanged) })
                    {
                        messageConsumer.Commit();
                        continue;
                    }

                    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
                    MainDbContext dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();

                    bool alreadyProcessed = await dbContext.ProcessedEvents
                        .AnyAsync(x => x.Id == integrationEvent.EventId, stoppingToken);

                    if (alreadyProcessed)
                    {
                        logger.Debug("Skipping duplicate event {EventId}", integrationEvent.EventId);
                        messageConsumer.Commit();
                        continue;
                    }

                    InventoryLevelChanged? levelChanged = JsonSerializer.Deserialize<InventoryLevelChanged>(integrationEvent.Payload);
                    if (levelChanged is null)
                    {
                        logger.Warning("Failed to deserialize inventory event.");
                        messageConsumer.Commit();
                        continue;
                    }

                    ProductVariant? variant = await dbContext.ProductVariants
                        .FirstOrDefaultAsync(x => x.Id == levelChanged.VariantId, stoppingToken);

                    if (variant is not null && variant.StockStatus != levelChanged.Status)
                    {
                        variant.StockStatus = levelChanged.Status;
                    }

                    ProcessedEvent @event = new() 
                    { 
                        Id = integrationEvent.EventId 
                    };

                    dbContext.ProcessedEvents.Add(@event);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    messageConsumer.Commit();

                    logger.Debug("Successfully processed and committed event {EventId}", integrationEvent.EventId);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error processing inventory message. Retrying in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.Information("Inventory consumer cancellation requested.");
        }
    }
}