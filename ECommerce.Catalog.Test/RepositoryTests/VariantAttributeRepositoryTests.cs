namespace ECommerce.Catalog.Test.RepositoryTests;

public class VariantAttributeRepositoryTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private readonly Mock<ILogger> _loggerMock;
    private readonly VariantAttributeRepository _repository;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public VariantAttributeRepositoryTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new MainDbContext(options);
        _loggerMock = new Mock<ILogger>();
        _repository = new VariantAttributeRepository(_dbContext, _loggerMock.Object);
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
    public async Task TestAddAsyncSuccessfullyInsertsAttribute()
    {
        // Arrange
        VariantAttributeModel attr = new() { Name = "Color", Value = "Red" };

        // Act
        VariantAttributeModel created = await _repository.AddAsync(attr, CancellationToken.None);

        // Assert
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Color", created.Name);
        Assert.Equal("Red", created.Value);
        Assert.Equal(1, _dbContext.VariantAttributes.Count());
    }

    [Fact]
    public async Task TestDeleteAsyncSuccessfullyRemovesAttribute()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        VariantAttributeModel attr = new() { Id = id, Name = "Color", Value = "Red", Version = version };

        _dbContext.VariantAttributes.Add(attr);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act
        VariantAttributeModel? deleted = await _repository.DeleteAsync(id, version, CancellationToken.None);

        // Assert
        Assert.NotNull(deleted);
        Assert.Equal(id, deleted.Id);
        Assert.Equal(0, _dbContext.VariantAttributes.Count());
    }

    [Fact]
    public async Task TestDeleteAsyncReturnsNullIfNotFound()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        VariantAttributeModel? deleted = await _repository.DeleteAsync(nonExistentId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(deleted);
        Assert.Equal(0, _dbContext.VariantAttributes.Count());
    }

    [Fact]
    public async Task TestUpdateAsyncForcesModifiedStateAndSaves()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        VariantAttributeModel attr = new() { Id = id, Name = "Color", Value = "Old", Version = version };

        _dbContext.VariantAttributes.Add(attr);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Simulate fetching and mutating in the service
        VariantAttributeModel tracked = (await _repository.GetAsync(id, CancellationToken.None))!;
        tracked.Value = "New";

        // Act - No assignment, just await
        await _repository.UpdateAsync(tracked, version, CancellationToken.None);

        // Assert - Assert against the tracked reference
        Assert.Equal("New", tracked.Value);

        VariantAttributeModel? dbRecord = await _dbContext.VariantAttributes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, CancellationToken.None);
        Assert.NotNull(dbRecord);
        Assert.Equal("New", dbRecord.Value);

        // VariantAttributeRepositoryTests logger verification
        _loggerMock.Verify(
            x => x.Debug(It.Is<string>(s => s.Contains("Executing UPDATE")), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task TestGetAsyncReturnsAttribute()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _dbContext.VariantAttributes.Add(new VariantAttributeModel { Id = id, Name = "Size", Value = "XL" });
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act
        VariantAttributeModel? result = await _repository.GetAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Size", result.Name);
        Assert.Equal("XL", result.Value);
    }

    [Fact]
    public async Task TestGetAsyncReturnsNullIfNotFound()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        VariantAttributeModel? result = await _repository.GetAsync(nonExistentId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task TestGetAllAsyncReturnsPaginatedResults()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            _dbContext.VariantAttributes.Add(new VariantAttributeModel { Id = Guid.NewGuid(), Name = $"Attr {i}", Value = $"Val {i}" });
        }
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act: Get page 2, 10 items per page
        PagedResult<VariantAttributeModel> result = await _repository.GetAllAsync(pageNumber: 2, itemsPerPage: 10, CancellationToken.None);

        // Assert
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.ItemsPerPage);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(5, result.Items.Count); // Page 2 should have the remaining 5 items
        Assert.Empty(_dbContext.ChangeTracker.Entries());
    }
}