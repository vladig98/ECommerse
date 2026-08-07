namespace ECommerce.Catalog.Test.ServicesTests;

public class KafkaMessageConsumerTests : IDisposable
{
    private readonly Mock<IConsumer<Ignore, ISpecificRecord>> _kafkaConsumerMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly KafkaMessageConsumer _consumerService;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public KafkaMessageConsumerTests()
    {
        _kafkaConsumerMock = new Mock<IConsumer<Ignore, ISpecificRecord>>();
        _loggerMock = new Mock<ILogger>();
        _consumerService = new KafkaMessageConsumer(_kafkaConsumerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void SubscribeCallsInnerConsumerSubscribe()
    {
        // Act
        _consumerService.Subscribe("inventory");

        // Assert
        _kafkaConsumerMock.Verify(x => x.Subscribe("inventory"), Times.Once);
    }

    [Fact]
    public void ConsumeReturnsNullWhenMessageIsNull()
    {
        // Arrange
        ConsumeResult<Ignore, ISpecificRecord> emptyResult = new();

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(emptyResult);

        // Act
        IntegrationEvent? result = _consumerService.Consume(CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ConsumeSuccessfullyParsesValidHeadersAndExtractsSchema()
    {
        // Arrange
        Guid expectedId = Guid.NewGuid();
        Headers headers = new()
        {
            { "eventId", Encoding.UTF8.GetBytes(expectedId.ToString()) }
        };

        Mock<ISpecificRecord> mockAvro = new();
        // FIX: Added ,"fields":[] to the JSON string
        mockAvro.Setup(x => x.Schema)
            .Returns(Avro.Schema.Parse("{\"type\":\"record\",\"name\":\"InventoryLevelChangedAvro\",\"namespace\":\"test\",\"fields\":[]}"));

        Message<Ignore, ISpecificRecord> message = new() { Value = mockAvro.Object, Headers = headers };
        ConsumeResult<Ignore, ISpecificRecord> consumeResult = new() { Message = message };

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        // Act
        IntegrationEvent? result = _consumerService.Consume(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.EventId);
        Assert.Equal("InventoryLevelChangedAvro", result.EventType);
        Assert.NotNull(result.Payload);
    }

    [Fact]
    public void ConsumeDefaultsIdAndLogsWarningWhenEventIdHeaderIsMissing()
    {
        // Arrange
        Mock<ISpecificRecord> mockAvro = new();
        // FIX: Added ,"fields":[] to the JSON string
        mockAvro.Setup(x => x.Schema)
            .Returns(Avro.Schema.Parse("{\"type\":\"record\",\"name\":\"InventoryLevelChangedAvro\",\"namespace\":\"test\",\"fields\":[]}"));

        Message<Ignore, ISpecificRecord> message = new() { Value = mockAvro.Object, Headers = [] };
        ConsumeResult<Ignore, ISpecificRecord> consumeResult = new() { Message = message };

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        // Act
        IntegrationEvent? result = _consumerService.Consume(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.EventId);
    }

    [Fact]
    public void ConsumeDefaultsIdAndLogsWarningWhenEventIdIsInvalid()
    {
        // Arrange
        Headers headers = new()
        {
            { "eventId", Encoding.UTF8.GetBytes("not-a-valid-guid") }
        };

        Mock<ISpecificRecord> mockAvro = new();
        // FIX: Added ,"fields":[] to the JSON string
        mockAvro.Setup(x => x.Schema).Returns(Avro.Schema.Parse("{\"type\":\"record\",\"name\":\"InventoryLevelChangedAvro\",\"namespace\":\"test\",\"fields\":[]}"));

        Message<Ignore, ISpecificRecord> message = new() { Value = mockAvro.Object, Headers = headers };
        ConsumeResult<Ignore, ISpecificRecord> consumeResult = new() { Message = message };

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        // Act
        IntegrationEvent? result = _consumerService.Consume(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.EventId);
        _loggerMock.Verify(x => x.Warning(It.Is<string>(s => s.Contains("invalid 'eventId' format"))), Times.Once);
    }

    [Fact]
    public void ConsumeReturnsNullAndLogsErrorOnConsumeException()
    {
        // Arrange
        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Throws(new ConsumeException(new ConsumeResult<byte[], byte[]>(), new Error(ErrorCode.BrokerNotAvailable)));

        // Act
        IntegrationEvent? result = _consumerService.Consume(CancellationToken.None);

        // Assert
        Assert.Null(result);
        _loggerMock.Verify(x => x.Error(It.IsAny<ConsumeException>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void CommitSuccessfullyCommitsLastResultAndClearsStateNullHeaders()
    {
        // Arrange
        Mock<ISpecificRecord> mockAvro = new();
        mockAvro.Setup(x => x.Schema).Returns(Avro.Schema.Parse("{\"type\":\"record\",\"name\":\"Test\",\"namespace\":\"test\",\"fields\":[]}"));

        Message<Ignore, ISpecificRecord> message = new() { Value = mockAvro.Object };
        ConsumeResult<Ignore, ISpecificRecord> consumeResult = new() { Message = message };

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(consumeResult);
        _consumerService.Consume(CancellationToken.None);

        // Act
        _consumerService.Commit();

        // Assert
        _kafkaConsumerMock.Verify(x => x.Commit(consumeResult), Times.Once);

        // Act again
        _consumerService.Commit();

        // Assert
        _kafkaConsumerMock.Verify(x => x.Commit(It.IsAny<ConsumeResult<Ignore, ISpecificRecord>>()), Times.Once);
    }

    [Fact]
    public void DisposeClosesAndDisposesInnerConsumer()
    {
        // Act
        _consumerService.Dispose();

        // Assert
        _kafkaConsumerMock.Verify(x => x.Close(), Times.Once);
        _kafkaConsumerMock.Verify(x => x.Dispose(), Times.Once);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed) return;
        if (disposing) _consumerService.Dispose();
        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }
        isDisposed = true;
    }
}