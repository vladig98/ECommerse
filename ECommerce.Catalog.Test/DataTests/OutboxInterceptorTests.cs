namespace ECommerce.Catalog.Test.DataTests;

public class OutboxInterceptorTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public OutboxInterceptorTests()
    {
        // Explicitly register the interceptor under test
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new OutboxInterceptor())
            .Options;

        _dbContext = new MainDbContext(options);
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
        }

        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }

        isDisposed = true;
    }

    [Fact]
    public async Task SavingChangesAsyncGeneratesProductCreatedEventWhenProductAdded()
    {
        // Arrange
        Category category = new() { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics" };
        Product product = new() { Id = Guid.NewGuid(), Title = "Laptop", Slug = "laptop", Category = category };

        _dbContext.Categories.Add(category);
        _dbContext.Products.Add(product);

        // Act
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Assert
        EventMessage? outboxMessage = await _dbContext.EventMessages
            .FirstOrDefaultAsync(x => x.Key == product.Id.ToString() && x.EventType == "ProductCreated", CancellationToken.None);

        Assert.NotNull(outboxMessage);
        Assert.Contains("Laptop", outboxMessage.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SavingChangesAsyncGeneratesProductUpdatedEventWhenProductModified()
    {
        // Arrange
        Category category = new() { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics" };
        Product product = new() { Id = Guid.NewGuid(), Title = "Laptop", Slug = "laptop", Category = category };

        _dbContext.Categories.Add(category);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Clear initial outbox messages generated from the Add
        _dbContext.EventMessages.RemoveRange(_dbContext.EventMessages);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        product.Title = "Updated Laptop";
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Assert
        EventMessage? outboxMessage = await _dbContext.EventMessages
            .FirstOrDefaultAsync(x => x.Key == product.Id.ToString() && x.EventType == "ProductUpdated", CancellationToken.None);

        Assert.NotNull(outboxMessage);
        Assert.Contains("Updated Laptop", outboxMessage.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SavingChangesAsyncGeneratesProductDeletedEventWhenProductDeleted()
    {
        // Arrange
        Category category = new() { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics" };
        Product product = new() { Id = Guid.NewGuid(), Title = "Laptop", Slug = "laptop", Category = category };

        _dbContext.Categories.Add(category);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Clear initial outbox messages
        _dbContext.EventMessages.RemoveRange(_dbContext.EventMessages);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Assert
        EventMessage? outboxMessage = await _dbContext.EventMessages
            .FirstOrDefaultAsync(x => x.Key == product.Id.ToString() && x.EventType == "ProductDeleted", CancellationToken.None);

        Assert.NotNull(outboxMessage);
        ProductDeleted? payload = JsonSerializer.Deserialize<ProductDeleted>(outboxMessage.Value);
        Assert.NotNull(payload);
        Assert.Equal(product.Id, payload.Id);
    }

    [Fact]
    public async Task SavingChangesAsyncGeneratesProductPriceChangedEventWhenVariantPriceModified()
    {
        // Arrange
        ProductVariant variant = new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Sku = "SKU-1", BasePrice = 100m };
        _dbContext.ProductVariants.Add(variant);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _dbContext.EventMessages.RemoveRange(_dbContext.EventMessages);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        variant.BasePrice = 150m; // Change the price
        _dbContext.ProductVariants.Update(variant);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Assert
        EventMessage? outboxMessage = await _dbContext.EventMessages
            .FirstOrDefaultAsync(x => x.Key == variant.ProductId.ToString() && x.EventType == "ProductPriceChanged", CancellationToken.None);

        Assert.NotNull(outboxMessage);
        ProductPriceChanged? payload = JsonSerializer.Deserialize<ProductPriceChanged>(outboxMessage.Value);
        Assert.NotNull(payload);
        Assert.Equal(150m, payload.NewPrice);
    }

    [Fact]
    public async Task SavingChangesAsyncIgnoresVariantModificationIfPriceDidNotChange()
    {
        // Arrange
        ProductVariant variant = new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Sku = "SKU-1", BasePrice = 100m, StockStatus = StockStatus.InStock };
        _dbContext.ProductVariants.Add(variant);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _dbContext.EventMessages.RemoveRange(_dbContext.EventMessages);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        variant.StockStatus = StockStatus.OutOfStock; // Change something OTHER than price
        _dbContext.ProductVariants.Update(variant);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Assert
        EventMessage? outboxMessage = await _dbContext.EventMessages
            .FirstOrDefaultAsync(x => x.Key == variant.ProductId.ToString() && x.EventType == "ProductPriceChanged", CancellationToken.None);

        Assert.Null(outboxMessage); // Price didn't change, so no event should be emitted
    }

    [Fact]
    public async Task SavingChangesAsyncGracefullyHandlesNullContext()
    {
        // Arrange
        AuditInterceptor interceptor = new(); // Or OutboxInterceptor

        // Pass null! for eventDefinition, messageGenerator, and context
        DbContextEventData eventData = new(
            eventDefinition: null!,
            messageGenerator: (_, _) => "Test",
            context: null);

        // Act & Assert
        // Should return cleanly without throwing NullReferenceException
        InterceptionResult<int> result = await interceptor.SavingChangesAsync(eventData, default, CancellationToken.None);

        Assert.Equal(default, result);
    }
}