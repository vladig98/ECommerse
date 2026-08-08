namespace ECommerce.Catalog.BackgroundServices;

public class OutboxPublisherService(
    IServiceProvider serviceProvider,
    IMessageProducer messageProducer,
    ILogger logger) : BackgroundService
{
    private const string topic = "products";
    private const int MaxRetries = 5;

    private readonly Dictionary<Guid, int> _messageRetries = [];

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
                    _messageRetries.Clear();
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                List<EventMessage> successfullyProcessed = [];
                int backoffDelaySeconds = 0;

                foreach (EventMessage msg in messages)
                {
                    try
                    {
                        ISpecificRecord avroMessage = msg.EventType switch
                        {
                            nameof(ProductCreated) => MapToProductCreatedAvro(msg.Value),
                            nameof(ProductUpdated) => MapToProductUpdatedAvro(msg.Value),
                            nameof(ProductDeleted) => MapToProductDeletedAvro(msg.Value),
                            nameof(ProductPriceChanged) => MapToProductPriceChangedAvro(msg.Value),
                            _ => throw new InvalidOperationException($"Unknown event type: {msg.EventType}")
                        };

                        await messageProducer.PublishAsync(topic, msg.Key, avroMessage, stoppingToken);

                        successfullyProcessed.Add(msg);
                        _messageRetries.Remove(msg.Id);
                    }
                    catch (Exception ex)
                    {
                        int retries = _messageRetries.GetValueOrDefault(msg.Id, 0) + 1;
                        _messageRetries[msg.Id] = retries;

                        if (retries >= MaxRetries)
                        {
                            logger.Error(ex, "Message {MessageId} failed {Max} times. Moving to DLQ.", msg.Id, MaxRetries);
                            await RouteSingleToDlqAsync(serviceProvider, msg, ex, stoppingToken);

                            successfullyProcessed.Add(msg);
                            _messageRetries.Remove(msg.Id);
                        }
                        else
                        {
                            logger.Warning(ex, "Failed to process message {MessageId}. Retry {Count}/{Max}", msg.Id, retries, MaxRetries);
                            backoffDelaySeconds = (int)Math.Pow(2, retries);
                            break;
                        }
                    }
                }

                if (successfullyProcessed.Count > 0)
                {
                    dbContext.EventMessages.RemoveRange(successfullyProcessed);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    logger.Debug("Published and cleared {Count} outbox messages.", successfullyProcessed.Count);
                }

                if (backoffDelaySeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(backoffDelaySeconds), stoppingToken);
                }
            }
            catch (Exception)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static async Task RouteSingleToDlqAsync(IServiceProvider sp, EventMessage msg, Exception exception, CancellationToken token)
    {
        await using AsyncServiceScope scope = sp.CreateAsyncScope();
        MainDbContext dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();

        DeadLetterMessage deadLetter = new()
        {
            Id = Guid.NewGuid(),
            OriginalMessageId = msg.Id,
            Source = "OutboxPublisher",
            Key = msg.Key,
            EventType = msg.EventType,
            Payload = msg.Value,
            ErrorReason = exception.Message,
            FailedAt = DateTime.UtcNow
        };

        dbContext.DeadLetterMessages.Add(deadLetter);
        await dbContext.SaveChangesAsync(token);
    }

    private static ProductDeletedAvro MapToProductDeletedAvro(string jsonValue)
    {
        ProductDeleted? domainEvent = JsonSerializer.Deserialize<ProductDeleted>(jsonValue)
            ?? throw new InvalidOperationException("Failed to deserialize ProductDeleted.");

        return new ProductDeletedAvro
        {
            Id = domainEvent.Id
        };
    }

    private static ProductPriceChangedAvro MapToProductPriceChangedAvro(string jsonValue)
    {
        ProductPriceChanged? domainEvent = JsonSerializer.Deserialize<ProductPriceChanged>(jsonValue)
            ?? throw new InvalidOperationException("Failed to deserialize ProductPriceChanged.");

        return new ProductPriceChangedAvro
        {
            ProductId = domainEvent.ProductId,
            VariantId = domainEvent.VariantId,
            Sku = domainEvent.Sku,
            NewPrice = CreateAvroDecimal(domainEvent.NewPrice, 2)
        };
    }

    private static Avro.AvroDecimal CreateAvroDecimal(decimal value, int scale)
    {
        // 1. Shift the decimal point to get the unscaled integer (e.g., 3.99 -> 399)
        decimal unscaled = Math.Round(value * (decimal)Math.Pow(10, scale));

        // 2. Convert directly to BigInteger
        System.Numerics.BigInteger bigIntValue = new(unscaled);

        // 3. Pass the BigInteger and scale directly into the constructor
        return new Avro.AvroDecimal(bigIntValue, scale);
    }

    private static ProductCreatedAvro MapToProductCreatedAvro(string jsonValue)
    {
        ProductCreated? e = JsonSerializer.Deserialize<ProductCreated>(jsonValue)
            ?? throw new InvalidOperationException("Failed to deserialize ProductCreated.");

        return new ProductCreatedAvro
        {
            Id = e.Id,
            Title = e.Title,
            Slug = e.Slug,
            Description = e.Description,
            Brand = e.Brand,
            IsActive = e.IsActive,
            Category = e.Category == null ? null : new CategoryEventDtoAvro
            {
                Id = e.Category.Id,
                Name = e.Category.Name,
                Slug = e.Category.Slug
            },
            Media = [.. e.Media.Select(m => new ProductMediaEventDtoAvro
            {
                Id = m.Id,
                Url = m.Url,
                AltText = m.AltText,
                Type = (int)m.Type,
                DisplayOrder = m.DisplayOrder,
                IsPrimary = m.IsPrimary
            })],
            Variants = [.. e.Variants.Select(v => new ProductVariantEventDtoAvro
            {
                Id = v.Id,
                Sku = v.Sku,
                BasePrice = CreateAvroDecimal(v.BasePrice, 2),
                Gtin = v.Gtin,
                Media = [.. v.Media.Select(m => new ProductMediaEventDtoAvro
                {
                    Id = m.Id,
                    Url = m.Url,
                    AltText = m.AltText,
                    Type = (int)m.Type,
                    DisplayOrder = m.DisplayOrder,
                    IsPrimary = m.IsPrimary
                })],
                Attributes = [.. v.Attributes.Select(a => new VariantAttributeEventDtoAvro
                {
                    Id = a.Id,
                    Name = a.Name,
                    Value = a.Value
                })]
            })]
        };
    }

    private static ProductUpdatedAvro MapToProductUpdatedAvro(string jsonValue)
    {
        ProductUpdated? e = JsonSerializer.Deserialize<ProductUpdated>(jsonValue)
            ?? throw new InvalidOperationException("Failed to deserialize ProductUpdated.");

        return new ProductUpdatedAvro
        {
            Id = e.Id,
            Title = e.Title,
            Slug = e.Slug,
            Description = e.Description,
            Brand = e.Brand,
            IsActive = e.IsActive,
            Category = e.Category == null ? null : new CategoryEventDtoAvro
            {
                Id = e.Category.Id,
                Name = e.Category.Name,
                Slug = e.Category.Slug
            },
            Media = [.. e.Media.Select(m => new ProductMediaEventDtoAvro
            {
                Id = m.Id,
                Url = m.Url,
                AltText = m.AltText,
                Type = (int)m.Type,
                DisplayOrder = m.DisplayOrder,
                IsPrimary = m.IsPrimary
            })],
            Variants = [.. e.Variants.Select(v => new ProductVariantEventDtoAvro
            {
                Id = v.Id,
                Sku = v.Sku,
                BasePrice = CreateAvroDecimal(v.BasePrice, 2),
                Gtin = v.Gtin,
                Media = [.. v.Media.Select(m => new ProductMediaEventDtoAvro
                {
                    Id = m.Id,
                    Url = m.Url,
                    AltText = m.AltText,
                    Type = (int)m.Type,
                    DisplayOrder = m.DisplayOrder,
                    IsPrimary = m.IsPrimary
                })],
                Attributes = [.. v.Attributes.Select(a => new VariantAttributeEventDtoAvro
                {
                    Id = a.Id,
                    Name = a.Name,
                    Value = a.Value
                })]
            })]
        };
    }
}