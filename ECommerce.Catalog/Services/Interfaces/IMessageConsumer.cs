namespace ECommerce.Catalog.Services.Interfaces;

internal interface IMessageConsumer : IDisposable
{
    void Subscribe(string topic);
    IntegrationEvent? Consume(CancellationToken token);
    void Commit();
}
