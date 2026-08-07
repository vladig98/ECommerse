namespace ECommerce.Catalog.Test.ServicesTests;

public class KafkaMessageProducerTests : IDisposable
{
    private readonly Mock<IProducer<string, ISpecificRecord>> _producerMock;
    private readonly KafkaMessageProducer _producerService;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public KafkaMessageProducerTests()
    {
        _producerMock = new Mock<IProducer<string, ISpecificRecord>>();
        _producerService = new KafkaMessageProducer(_producerMock.Object);
    }

    [Fact]
    public async Task PublishAsyncSuccessfullyBuildsAndSendsKafkaMessage()
    {
        // Arrange
        string topic = "products";
        string key = "product-123";
        Mock<ISpecificRecord> mockAvroEvent = new();

        _producerMock.Setup(x => x.ProduceAsync(topic, It.IsAny<Message<string, ISpecificRecord>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, ISpecificRecord>());

        // Act: Use 4 parameters, passing the avro event directly
        await _producerService.PublishAsync(topic, key, mockAvroEvent.Object, CancellationToken.None);

        // Assert
        // Verify that the inner Kafka producer is called with the key and exact ISpecificRecord
        _producerMock.Verify(x => x.ProduceAsync(
            topic,
            It.Is<Message<string, ISpecificRecord>>(m =>
                m.Key == key &&
                m.Value == mockAvroEvent.Object
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
        if (isDisposed) return;
        if (disposing) _producerService.Dispose();
        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }
        isDisposed = true;
    }
}