namespace ECommerce.Catalog.Services.Interfaces;

public interface IMessageProducer
{
    Task PublishAsync<T>(string topic, string key, T message, CancellationToken token) where T : ISpecificRecord;
}
