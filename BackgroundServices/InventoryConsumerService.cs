namespace ECommerce.Catalog.BackgroundServices;

public class InventoryConsumerService(
    IServiceProvider serviceProvider,
    IMessageConsumer messageConsumer,
    ILogger logger) : BackgroundService
{
    private const string topic = "inventory";
    private const int MaxRetries = 5;

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

        int retryCount = 0;
        IntegrationEvent? pendingEvent = null;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (pendingEvent is null)
                    {
                        pendingEvent = messageConsumer.Consume(stoppingToken);
                        if (pendingEvent is null)
                        {
                            continue;
                        }
                    }

                    if (pendingEvent is not { EventType: nameof(InventoryLevelChangedAvro) })
                    {
                        CommitAndReset(ref pendingEvent, ref retryCount);
                        continue;
                    }

                    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
                    MainDbContext dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();

                    bool alreadyProcessed = await dbContext.ProcessedEvents
                        .AnyAsync(x => x.Id == pendingEvent.EventId, stoppingToken);

                    if (alreadyProcessed)
                    {
                        logger.Debug("Skipping duplicate event {EventId}", pendingEvent.EventId);
                        CommitAndReset(ref pendingEvent, ref retryCount);
                        continue;
                    }

                    if (pendingEvent.Payload is not InventoryLevelChangedAvro levelChanged)
                    {
                        throw new InvalidOperationException("Payload was not of type InventoryLevelChangedAvro.");
                    }

                    ProductVariant? variant = await dbContext.ProductVariants
                        .FirstOrDefaultAsync(x => x.Id == levelChanged.VariantId, stoppingToken);

                    if (variant is not null && variant.StockStatus != (StockStatus)levelChanged.Status)
                    {
                        variant.StockStatus = (StockStatus)levelChanged.Status;
                    }

                    dbContext.ProcessedEvents.Add(new ProcessedEvent { Id = pendingEvent.EventId });
                    await dbContext.SaveChangesAsync(stoppingToken);

                    logger.Debug("Successfully processed and committed event {EventId}", pendingEvent.EventId);
                    CommitAndReset(ref pendingEvent, ref retryCount);
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= MaxRetries)
                    {
                        logger.Error(ex, "Poison message detected {EventId} after {Max} retries. Routing to DLQ.", pendingEvent?.EventId, MaxRetries);

                        await using AsyncServiceScope dlqScope = serviceProvider.CreateAsyncScope();
                        MainDbContext dlqContext = dlqScope.ServiceProvider.GetRequiredService<MainDbContext>();

                        DeadLetterMessage dlqMessage = new()
                        {
                            Id = Guid.NewGuid(),
                            OriginalMessageId = pendingEvent?.EventId,
                            Source = "InventoryConsumer",
                            EventType = pendingEvent?.EventType ?? "Unknown",
                            Payload = pendingEvent?.Payload != null
                                ? JsonSerializer.Serialize((object)pendingEvent.Payload)
                                : "{}",
                            ErrorReason = ex.Message,
                            FailedAt = DateTime.UtcNow
                        };

                        dlqContext.DeadLetterMessages.Add(dlqMessage);
                        await dlqContext.SaveChangesAsync(stoppingToken);

                        CommitAndReset(ref pendingEvent, ref retryCount);
                    }
                    else
                    {
                        // Exponential backoff: 2, 4, 8, 16 seconds...
                        int backoffSeconds = (int)Math.Pow(2, retryCount);
                        logger.Warning(ex, "Error processing inventory message. Retrying in {Delay} seconds...", backoffSeconds);
                        await Task.Delay(TimeSpan.FromSeconds(backoffSeconds), stoppingToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.Information("Inventory consumer cancellation requested.");
        }
    }

    private void CommitAndReset(ref IntegrationEvent? pendingEvent, ref int retryCount)
    {
        messageConsumer.Commit();
        pendingEvent = null;
        retryCount = 0;
    }
}