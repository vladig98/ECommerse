namespace ECommerce.Catalog.Events;

public record class IntegrationEvent
(
    Guid EventId,
    string Key,
    string EventType,
    ISpecificRecord Payload
);