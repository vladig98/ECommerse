namespace ECommerce.Catalog.Test.DataTests;

public class AuditInterceptorTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public AuditInterceptorTests()
    {
        // Explicitly register the interceptor under test
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(new AuditInterceptor())
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
    public async Task SavingChangesAsyncSetsAuditPropertiesOnAddedEntity()
    {
        // Arrange
        Category newCategory = new()
        {
            Name = "Test",
            Slug = "test",
            CreatedAt = DateTime.MinValue,
            UpdatedAt = DateTime.MinValue,
            Version = Guid.Empty
        };

        _dbContext.Categories.Add(newCategory);

        // Act
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Assert
        Assert.NotEqual(DateTime.MinValue, newCategory.CreatedAt);
        Assert.NotEqual(DateTime.MinValue, newCategory.UpdatedAt);
        Assert.NotEqual(Guid.Empty, newCategory.Version);

        // CreatedAt and UpdatedAt should be practically identical
        Assert.Equal(newCategory.CreatedAt, newCategory.UpdatedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SavingChangesAsyncUpdatesAuditPropertiesOnModifiedEntity()
    {
        // Arrange
        Category category = new() { Name = "Test", Slug = "test" };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        DateTime originalCreatedAt = category.CreatedAt;
        DateTime originalUpdatedAt = category.UpdatedAt;
        Guid originalVersion = category.Version;

        // Simulate some time passing
        await Task.Delay(100, CancellationToken.None);

        // Act
        category.Name = "Updated Test";
        _dbContext.Categories.Update(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Assert
        Assert.Equal(originalCreatedAt, category.CreatedAt); // CreatedAt MUST NOT change
        Assert.NotEqual(originalUpdatedAt, category.UpdatedAt); // UpdatedAt MUST change
        Assert.NotEqual(originalVersion, category.Version); // Version MUST change
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