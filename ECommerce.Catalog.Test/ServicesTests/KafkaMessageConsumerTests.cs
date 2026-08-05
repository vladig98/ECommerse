namespace ECommerce.Catalog.Test.ServicesTests;

public class KafkaMessageConsumerTests : IDisposable
{
    private readonly Mock<IConsumer<Ignore, string>> _kafkaConsumerMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly KafkaMessageConsumer _consumerService;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public KafkaMessageConsumerTests()
    {
        _kafkaConsumerMock = new Mock<IConsumer<Ignore, string>>();
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
        ConsumeResult<Ignore, string> emptyResult = new();

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(emptyResult);

        // Act
        IntegrationEvent? result = _consumerService.Consume(CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ConsumeSuccessfullyParsesValidHeaders()
    {
        // Arrange
        Guid expectedId = Guid.NewGuid();
        Headers headers = new()
        {
            { "eventType", Encoding.UTF8.GetBytes("InventoryLevelChanged") },
            { "eventId", Encoding.UTF8.GetBytes(expectedId.ToString()) }
        };

        Message<Ignore, string> message = new() { Value = "{}", Headers = headers };
        ConsumeResult<Ignore, string> consumeResult = new() { Message = message };

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        // Act
        IntegrationEvent? result = _consumerService.Consume(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.EventId);
        Assert.Equal("InventoryLevelChanged", result.EventType);
        Assert.Equal("{}", result.Payload);
    }

    [Fact]
    public void ConsumeDefaultsIdAndLogsWarningWhenEventIdHeaderIsMissing()
    {
        // Arrange
        Headers headers = new()
        {
            { "eventType", Encoding.UTF8.GetBytes("InventoryLevelChanged") }
            // Intentionally missing eventId
        };

        Message<Ignore, string> message = new() { Value = "{}", Headers = headers };
        ConsumeResult<Ignore, string> consumeResult = new() { Message = message };

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(consumeResult);

        // Act
        IntegrationEvent? result = _consumerService.Consume(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.EventId); // Should have generated a new Guid fallback
        _loggerMock.Verify(x => x.Warning(It.Is<string>(s => s.Contains("without an 'eventId' header"))), Times.Once);
    }

    [Fact]
    public void ConsumeDefaultsIdAndLogsWarningWhenEventIdIsInvalid()
    {
        // Arrange
        Headers headers = new()
        {
            { "eventType", Encoding.UTF8.GetBytes("InventoryLevelChanged") },
            { "eventId", Encoding.UTF8.GetBytes("not-a-valid-guid") } // Corrupted data
        };

        Message<Ignore, string> message = new() { Value = "{}", Headers = headers };
        ConsumeResult<Ignore, string> consumeResult = new() { Message = message };

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
        _loggerMock.Verify(x => x.Error(It.IsAny<ConsumeException>(), "Kafka consume error."), Times.Once);
    }

    [Fact]
    public void CommitSuccessfullyCommitsLastResultAndClearsStateNullHeaders()
    {
        // Arrange
        Message<Ignore, string> message = new()
        {
            Value = "{}"
        };
        ConsumeResult<Ignore, string> consumeResult = new() { Message = message };

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(consumeResult);

        // Populate _lastResult
        _consumerService.Consume(CancellationToken.None);

        // Act
        _consumerService.Commit();

        // Assert: It should have committed the result
        _kafkaConsumerMock.Verify(x => x.Commit(consumeResult), Times.Once);

        // Act again
        _consumerService.Commit();

        // Assert: It should NOT commit again because state was cleared to null
        _kafkaConsumerMock.Verify(x => x.Commit(It.IsAny<ConsumeResult<Ignore, string>>()), Times.Once);
    }

    [Fact]
    public void CommitSuccessfullyCommitsLastResultAndClearsState()
    {
        // Arrange
        Message<Ignore, string> message = new()
        {
            Value = "{}",
            Headers = []
        };
        ConsumeResult<Ignore, string> consumeResult = new() { Message = message };

        _kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(consumeResult);

        // Populate _lastResult
        _consumerService.Consume(CancellationToken.None);

        // Act
        _consumerService.Commit();

        // Assert: It should have committed the result
        _kafkaConsumerMock.Verify(x => x.Commit(consumeResult), Times.Once);

        // Act again
        _consumerService.Commit();

        // Assert: It should NOT commit again because state was cleared to null
        _kafkaConsumerMock.Verify(x => x.Commit(It.IsAny<ConsumeResult<Ignore, string>>()), Times.Once);
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
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            _consumerService.Dispose();
        }

        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }

        isDisposed = true;
    }
}