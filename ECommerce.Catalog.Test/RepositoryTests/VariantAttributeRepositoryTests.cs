namespace ECommerce.Catalog.Test.RepositoryTests;

public class VariantAttributeRepositoryTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private readonly Mock<ILogger> _loggerMock;
    private readonly VariantAttributeRepository _repository;

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
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Test_AddAsync()
    {
        // Arrange
        VariantAttribute attr = new() { Name = "Color", Value = "Red" };

        // Act
        VariantAttribute created = await _repository.AddAsync(attr, CancellationToken.None);

        // Assert
        Assert.NotNull(created);
        Assert.Equal(1, _dbContext.VariantAttributes.Count());
    }

    [Fact]
    public async Task Test_DeleteAsync()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        VariantAttribute attr = new() { Id = id, Name = "Color", Value = "Red", Version = version };

        _dbContext.VariantAttributes.Add(attr);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act
        VariantAttribute? deleted = await _repository.DeleteAsync(id, version, CancellationToken.None);

        // Assert
        Assert.NotNull(deleted);
        Assert.Equal(0, _dbContext.VariantAttributes.Count());
    }

    [Fact]
    public async Task Test_UpdateAsync()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        VariantAttribute attr = new() { Id = id, Name = "Color", Value = "Old", Version = version };

        _dbContext.VariantAttributes.Add(attr);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        VariantAttribute tracked = (await _repository.GetAsync(id, CancellationToken.None))!;
        tracked.Value = "New";

        // Act
        await _repository.UpdateAsync(tracked, version, CancellationToken.None);

        // Assert
        VariantAttribute? dbRecord = await _dbContext.VariantAttributes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, CancellationToken.None);
        Assert.Equal("New", dbRecord!.Value);
    }
}