namespace ECommerce.Catalog.Test.BackgroundServices;

public class InventoryConsumerServiceTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private readonly Mock<IMessageConsumer> _consumerMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly InventoryConsumerService _service;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public InventoryConsumerServiceTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new MainDbContext(options);
        _consumerMock = new Mock<IMessageConsumer>();
        _loggerMock = new Mock<ILogger>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        // Mock the IServiceScopeFactory hierarchy
        Mock<IServiceScopeFactory> scopeFactoryMock = new();
        Mock<IServiceScope> scopeMock = new();

        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

        // Inject the In-Memory DbContext
        _serviceProviderMock.Setup(x => x.GetService(typeof(MainDbContext))).Returns(_dbContext);

        _service = new InventoryConsumerService(_serviceProviderMock.Object, _consumerMock.Object, _loggerMock.Object);

        // FIX: Pre-warm EF Core to force model compilation immediately, eliminating cold-start timeouts
        _dbContext.Database.EnsureCreated();
        _dbContext.ProcessedEvents.AnyAsync(CancellationToken.None).Wait(CancellationToken.None);
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
    public async Task ExecuteAsyncSuccessfullyUpdatesStockAndCommits()
    {
        // Arrange
        Guid variantId = Guid.NewGuid();
        Guid eventId = Guid.NewGuid();

        _dbContext.ProductVariants.Add(new ProductVariant { Id = variantId, Sku = "SKU-1", StockStatus = StockStatus.InStock });
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        string payload = JsonSerializer.Serialize(new InventoryLevelChanged(variantId, "SKU-1", StockStatus.OutOfStock));
        IntegrationEvent incomingEvent = new(eventId, "key", "InventoryLevelChanged", payload);

        // Return event on the first loop, then throw OperationCanceledException on the second loop
        // to force the background thread into its Task.Delay which cleanly responds to cts.Cancel()
        _consumerMock.SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(incomingEvent)
            .Throws(new OperationCanceledException(CancellationToken.None));

        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None); // Wait half a second to guarantee completion
        await cts.CancelAsync();
        await _service.StopAsync(CancellationToken.None);

        // Assert
        ProductVariant? variant = await _dbContext.ProductVariants.FindAsync([variantId], cancellationToken: CancellationToken.None);
        Assert.NotNull(variant);
        Assert.Equal(StockStatus.OutOfStock, variant.StockStatus);

        bool isProcessed = await _dbContext.ProcessedEvents.AnyAsync(x => x.Id == eventId, CancellationToken.None);
        Assert.True(isProcessed);

        _consumerMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncIgnoresAlreadyProcessedEvents()
    {
        // Arrange
        Guid eventId = Guid.NewGuid();

        _dbContext.ProcessedEvents.Add(new ProcessedEvent { Id = eventId });
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        IntegrationEvent incomingEvent = new(eventId, "key", "InventoryLevelChanged", "{}");

        _consumerMock.SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(incomingEvent)
            .Throws(new OperationCanceledException(CancellationToken.None));

        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        await cts.CancelAsync();
        await _service.StopAsync(CancellationToken.None);

        // Assert
        _consumerMock.Verify(x => x.Commit(), Times.Once);
        _loggerMock.Verify(x => x.Debug("Skipping duplicate event {EventId}", eventId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncIgnoresIrrelevantEventTypes()
    {
        // Arrange
        IntegrationEvent incomingEvent = new(Guid.NewGuid(), "key", "SomeOtherEvent", "{}");

        _consumerMock.SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(incomingEvent)
            .Throws(new OperationCanceledException(CancellationToken.None));

        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        await cts.CancelAsync();
        await _service.StopAsync(CancellationToken.None);

        // Assert
        _consumerMock.Verify(x => x.Commit(), Times.Once);
        _loggerMock.Verify(x => x.Debug(It.Is<string>(s => s.Contains("Successfully processed")), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsyncHandlesDeserializationFailures()
    {
        // Arrange
        IntegrationEvent incomingEvent = new(Guid.NewGuid(), "key", "InventoryLevelChanged", "null");

        _consumerMock.SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(incomingEvent)
            .Throws(new OperationCanceledException(CancellationToken.None));

        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        await cts.CancelAsync();
        await _service.StopAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(x => x.Warning("Failed to deserialize inventory event."), Times.Once);
        _consumerMock.Verify(x => x.Commit(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncCatchesExceptionsAndRetries()
    {
        // Arrange
        IntegrationEvent incomingEvent = new(Guid.NewGuid(), "key", "InventoryLevelChanged", "{}");

        _serviceProviderMock.Setup(x => x.GetService(typeof(MainDbContext))).Throws(new TimeoutException("Database down"));

        _consumerMock.SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(incomingEvent)
            .Throws(new OperationCanceledException(CancellationToken.None));

        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(500, CancellationToken.None);
        await cts.CancelAsync();
        await _service.StopAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(x => x.Error(It.IsAny<Exception>(), "Error processing inventory message. Retrying in 5 seconds..."), Times.Once);
        _consumerMock.Verify(x => x.Commit(), Times.Never);
    }
}