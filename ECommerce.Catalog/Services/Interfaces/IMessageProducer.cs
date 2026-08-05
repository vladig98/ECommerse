namespace ECommerce.Catalog.Services.Interfaces;

internal interface IMessageProducer
{
    Task PublishAsync(string topic, IntegrationEvent message, CancellationToken token);
}
