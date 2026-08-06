namespace ECommerce.Catalog.Services.Interfaces;

public interface IMessageProducer
{
    Task PublishAsync(string topic, IntegrationEvent message, CancellationToken token);
}
