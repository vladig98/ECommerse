namespace ECommerce.Catalog.BackgroundServices;

public interface IEventProducer
{
    Task ProduceAsync(CancellationToken token);
}
