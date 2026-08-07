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

        // Pre-warm EF Core to force model compilation immediately, eliminating cold-start timeouts
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

        // Pass the actual Avro generated object
        InventoryLevelChangedAvro payload = new()
        {
            VariantId = variantId,
            Sku = "SKU-1",
            Status = (int)StockStatus.OutOfStock
        };

        IntegrationEvent incomingEvent = new(eventId, "key", nameof(InventoryLevelChangedAvro), payload);

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

        InventoryLevelChangedAvro payload = new() { VariantId = Guid.NewGuid(), Sku = "SKU-1", Status = 1 };
        IntegrationEvent incomingEvent = new(eventId, "key", nameof(InventoryLevelChangedAvro), payload);

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
        Mock<ISpecificRecord> dummyPayload = new();
        IntegrationEvent incomingEvent = new(Guid.NewGuid(), "key", "SomeOtherEventAvro", dummyPayload.Object);

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
    public async Task ExecuteAsyncThrowsAndRetriesOnBadPayloadType()
    {
        // Arrange
        Mock<ISpecificRecord> wrongPayloadType = new(); // Not InventoryLevelChangedAvro
        IntegrationEvent incomingEvent = new(Guid.NewGuid(), "key", nameof(InventoryLevelChangedAvro), wrongPayloadType.Object);

        _consumerMock.SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(incomingEvent)
            .Throws(new OperationCanceledException(CancellationToken.None));

        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(200, CancellationToken.None);
        await cts.CancelAsync();
        await _service.StopAsync(CancellationToken.None);

        // Assert: Casting failure should be caught by the general exception handler and trigger exponential backoff
        _loggerMock.Verify(x => x.Warning(It.IsAny<Exception>(), "Error processing inventory message. Retrying in {Delay} seconds...", It.IsAny<int>()), Times.AtLeastOnce);
        _consumerMock.Verify(x => x.Commit(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsyncCatchesExceptionsAndRetriesWithExponentialBackoff()
    {
        // Arrange
        InventoryLevelChangedAvro payload = new() { VariantId = Guid.NewGuid(), Sku = "SKU-1", Status = 1 };
        IntegrationEvent incomingEvent = new(Guid.NewGuid(), "key", nameof(InventoryLevelChangedAvro), payload);

        // Simulate database outage
        _serviceProviderMock.Setup(x => x.GetService(typeof(MainDbContext))).Throws(new TimeoutException("Database down"));

        _consumerMock.SetupSequence(x => x.Consume(It.IsAny<CancellationToken>()))
            .Returns(incomingEvent)
            .Throws(new OperationCanceledException(CancellationToken.None));

        using CancellationTokenSource cts = new();

        // Act
        await _service.StartAsync(cts.Token);
        await Task.Delay(200, CancellationToken.None);
        await cts.CancelAsync();
        await _service.StopAsync(CancellationToken.None);

        // Assert: We now verify the new Serilog Warning for exponential backoff instead of the old hardcoded Error
        _loggerMock.Verify(x => x.Warning(It.IsAny<Exception>(), "Error processing inventory message. Retrying in {Delay} seconds...", It.IsAny<int>()), Times.AtLeastOnce);
        _consumerMock.Verify(x => x.Commit(), Times.Never);
    }
}