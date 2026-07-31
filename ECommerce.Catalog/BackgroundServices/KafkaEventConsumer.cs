namespace ECommerce.Catalog.BackgroundServices;

public class KafkaEventConsumer(
    IOptions<KafkaSettings> options,
    IServiceProvider serviceProvider,
    ILogger logger) : BackgroundService
{
    private const string topic = "inventory";
    private readonly ConsumerConfig config = new()
    {
        BootstrapServers = options.Value.Server,
        SaslUsername = options.Value.Username,
        SaslPassword = options.Value.Password,
        SecurityProtocol = SecurityProtocol.SaslSsl,
        SaslMechanism = SaslMechanism.Plain,
        GroupId = "catalog-service-inventory-consumer",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Information("Starting Kafka Inventory Consumer...");

        using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<Ignore, string> consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult.Message.Headers.TryGetLastBytes("eventType", out byte[]? eventTypeBytes))
                    {
                        string eventType = Encoding.UTF8.GetString(eventTypeBytes);
                        if (eventType != nameof(InventoryLevelChanged))
                        {
                            consumer.Commit(consumeResult);
                            continue;
                        }
                    }

                    InventoryLevelChanged? levelChanged = JsonSerializer.Deserialize<InventoryLevelChanged>(consumeResult.Message.Value);
                    if (levelChanged is null)
                    {
                        logger.Warning("Failed to deserialize inventory event.");
                        consumer.Commit(consumeResult);
                        continue;
                    }

                    await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
                    MainDbContext mainDbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();

                    ProductVariant? variant = await mainDbContext.ProductVariants
                        .FirstOrDefaultAsync(x => x.Id == levelChanged.VariantId, stoppingToken);

                    if (variant is not null && variant.StockStatus != levelChanged.Status)
                    {
                        variant.StockStatus = levelChanged.Status;
                        await mainDbContext.SaveChangesAsync(stoppingToken);
                        logger.Debug("Updated stock status for variant {VariantId} to {Status}", variant.Id, variant.StockStatus);
                    }

                    consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    logger.Error(ex, "Kafka consume error.");
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
        finally
        {
            consumer.Close();
        }
    }
}