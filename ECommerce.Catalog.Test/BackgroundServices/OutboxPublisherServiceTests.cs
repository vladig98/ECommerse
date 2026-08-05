namespace ECommerce.Catalog.Test.BackgroundServices;

public class OutboxPublisherServiceTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private readonly Mock<IMessageProducer> _producerMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly OutboxPublisherService _service;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);
    public OutboxPublisherServiceTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new MainDbContext(options);
        _producerMock = new Mock<IMessageProducer>();
        _loggerMock = new Mock<ILogger>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        // Mock the IServiceScopeFactory hierarchy for CreateAsyncScope()
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();

        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

        // Inject the In-Memory DbContext whenever the scope asks for it
        _serviceProviderMock.Setup(x => x.GetService(typeof(MainDbContext))).Returns(_dbContext);

        _service = new OutboxPublisherService(_serviceProviderMock.Object, _producerMock.Object, _loggerMock.Object);
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
            _dbContext.Dispose();
            _service.Dispose();
        }

        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }

        isDisposed = true;
    }

    [Fact]
    public async Task ExecuteAsyncSuccessfullyPublishesAndRemovesMessages()
    {
        // Arrange
        Guid eventId = Guid.NewGuid();
        _dbContext.EventMessages.Add(new EventMessage
        {
            Id = eventId,
            Key = "key1",
            EventType = "ProductCreated",
            Value = "{}"
        });
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _producerMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(200, CancellationToken.None); // Allow the background loop to process
        await cts.CancelAsync(); // Signal the loop to stop

        // Suppress expected cancellation exceptions during teardown
        await _service.StopAsync(CancellationToken.None);

        // Assert
        // 1. Verify MessageProducer was invoked with correct data
        _producerMock.Verify(x => x.PublishAsync("products", It.Is<IntegrationEvent>(e =>
            e.EventId == eventId &&
            e.EventType == "ProductCreated" &&
            e.Payload == "{}"), It.IsAny<CancellationToken>()), Times.Once);

        // 2. Verify the database was cleaned up
        int remainingMessages = await _dbContext.EventMessages.CountAsync(CancellationToken.None);
        Assert.Equal(0, remainingMessages);
    }

    [Fact]
    public async Task ExecuteAsyncDelaysWhenNoMessagesFound()
    {
        // Arrange (Empty Database)
        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(100, CancellationToken.None);
        await cts.CancelAsync();
        await _service.StopAsync(CancellationToken.None);

        // Assert
        _producerMock.Verify(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsyncCatchesExceptionsAndLogsError()
    {
        // Arrange
        _dbContext.EventMessages.Add(new EventMessage { Id = Guid.NewGuid(), Key = "key1", EventType = "ProductCreated", Value = "{}" });
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Simulate a Kafka outage
        _producerMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<IntegrationEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Kafka Broker Down"));

        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(200, CancellationToken.None);
        await cts.CancelAsync();
        await _service.StopAsync(CancellationToken.None);

        // Assert
        // Verify the error was caught and logged (meaning the service didn't crash)
        _loggerMock.Verify(x => x.Error(It.IsAny<Exception>(), "Failed to process outbox messages. Retrying in 10 seconds..."), Times.AtLeastOnce);

        // Verify the message was NOT deleted from the database
        int remainingMessages = await _dbContext.EventMessages.CountAsync(CancellationToken.None);
        Assert.Equal(1, remainingMessages);
    }
}