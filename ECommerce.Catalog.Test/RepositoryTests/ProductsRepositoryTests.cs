namespace ECommerce.Catalog.Test.RepositoryTests;

public class ProductsRepositoryTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private readonly Mock<ILogger> _loggerMock;
    private readonly ProductsRepository _productsRepository;

    public ProductsRepositoryTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new MainDbContext(options);
        _loggerMock = new Mock<ILogger>(CancellationToken.None);
        _productsRepository = new ProductsRepository(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Test_AddAsync()
    {
        // Arrange
        Product product = new() { Title = "Laptop", Slug = "laptop", CategoryId = Guid.NewGuid() };

        // Act
        Product created = await _productsRepository.AddAsync(product, CancellationToken.None);

        // Assert
        Assert.NotNull(created);
        Assert.Equal(1, _dbContext.Products.Count());
    }

    [Fact]
    public async Task Test_DeleteAsync()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        Product product = new() { Id = id, Title = "Laptop", Slug = "laptop", Version = version };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act
        Product? deleted = await _productsRepository.DeleteAsync(id, version, CancellationToken.None);

        // Assert
        Assert.NotNull(deleted);
        Assert.Equal(0, _dbContext.Products.Count());
    }

    [Fact]
    public async Task Test_UpdateAsync()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        Product product = new() { Id = id, Title = "Old", Slug = "old", Version = version };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        Product tracked = (await _productsRepository.GetAsync(id, CancellationToken.None))!;
        tracked.Title = "New";

        // Act
        await _productsRepository.UpdateAsync(tracked, version, CancellationToken.None);

        // Assert
        Product? dbRecord = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, CancellationToken.None);
        Assert.Equal("New", dbRecord!.Title);
    }

    [Fact]
    public async Task Test_GetAllAsync()
    {
        // Arrange
        _dbContext.Products.Add(new Product { Id = Guid.NewGuid(), Title = "A", Slug = "a" });
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act
        PagedResult<Product> result = await _productsRepository.GetAllAsync(1, 10, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
    }
}