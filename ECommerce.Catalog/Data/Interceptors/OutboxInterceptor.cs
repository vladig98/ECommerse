namespace ECommerce.Catalog.Data.Interceptors;

public class OutboxInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        DbContext? context = eventData.Context;
        if (context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        DateTime utcNow = DateTime.UtcNow;
        List<EventMessage> outboxMessages = [];

        foreach (EntityEntry<BaseModel> entry in context.ChangeTracker.Entries<BaseModel>())
        {
            if (entry.Entity is Product product)
            {
                if (entry.State == EntityState.Added)
                {
                    outboxMessages.Add(CreateEventMessage(product.Id, nameof(ProductCreated), product.ToEventData()));
                }
                else if (entry.State == EntityState.Modified)
                {
                    outboxMessages.Add(CreateEventMessage(product.Id, nameof(ProductUpdated), product.ToEventDataUpdate()));
                }
                else if (entry.State == EntityState.Deleted)
                {
                    outboxMessages.Add(CreateEventMessage(product.Id, nameof(ProductDeleted), product.ToEventDataDelete()));
                }
            }
            else if (entry.Entity is ProductVariant variant && entry.State == EntityState.Modified)
            {
                decimal originalPrice = entry.OriginalValues.GetValue<decimal>(nameof(ProductVariant.BasePrice));
                decimal currentPrice = entry.CurrentValues.GetValue<decimal>(nameof(ProductVariant.BasePrice));

                if (originalPrice != currentPrice)
                {
                    outboxMessages.Add(CreateEventMessage(
                        aggregateId: variant.ProductId,
                        eventType: nameof(ProductPriceChanged),
                        payload: variant.ToPricheChangeEventData()
                    ));
                }
            }
        }

        if (outboxMessages.Count > 0)
        {
            context.Set<EventMessage>().AddRange(outboxMessages);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static EventMessage CreateEventMessage<T>(Guid aggregateId, string eventType, T payload)
    {
        DateTime now = DateTime.UtcNow;
        return new EventMessage
        {
            Id = Guid.NewGuid(),
            Key = aggregateId.ToString(),
            EventType = eventType,
            Value = JsonSerializer.Serialize(payload),
            CreatedAt = now,
            UpdatedAt = now,
            Version = Guid.NewGuid()
        };
    }
}
