namespace ECommerce.Catalog.Events;

internal record class IntegrationEvent
(
    Guid EventId,
    string Key,
    string EventType,
    string Payload
);