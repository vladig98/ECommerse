namespace ECommerce.Catalog.Test.ServicesTests;

public class KafkaMessageProducerTests : IDisposable
{
    private readonly Mock<IProducer<string, string>> _producerMock;
    private readonly KafkaMessageProducer _producerService;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public KafkaMessageProducerTests()
    {
        _producerMock = new Mock<IProducer<string, string>>();
        _producerService = new KafkaMessageProducer(_producerMock.Object);
    }

    [Fact]
    public async Task PublishAsyncSuccessfullyBuildsAndSendsKafkaMessage()
    {
        // Arrange
        string topic = "products";
        Guid eventId = Guid.NewGuid();
        string key = "product-123";
        string eventType = "ProductUpdated";
        string payload = "{\"name\":\"test\"}";

        IntegrationEvent integrationEvent = new(eventId, key, eventType, payload);

        _producerMock.Setup(x => x.ProduceAsync(topic, It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        // Act
        await _producerService.PublishAsync(topic, integrationEvent, CancellationToken.None);

        // Assert
        // Verify that the inner Kafka producer is called with a meticulously constructed message
        _producerMock.Verify(x => x.ProduceAsync(
            topic,
            It.Is<Message<string, string>>(m =>
                m.Key == key &&
                m.Value == payload &&
                Encoding.UTF8.GetString(m.Headers.GetLastBytes("eventType")) == eventType &&
                Encoding.UTF8.GetString(m.Headers.GetLastBytes("eventId")) == eventId.ToString()
            ),
            CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public void DisposeSuccessfullyDisposesInnerProducer()
    {
        // Act
        _producerService.Dispose();

        // Assert
        _producerMock.Verify(x => x.Dispose(), Times.Once);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            _producerService.Dispose();
        }

        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }

        isDisposed = true;
    }
}