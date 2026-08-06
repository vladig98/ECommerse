namespace ECommerce.Catalog.Services.Interfaces;

public interface IMessageConsumer : IDisposable
{
    void Subscribe(string topic);
    IntegrationEvent? Consume(CancellationToken token);
    void Commit();
}
