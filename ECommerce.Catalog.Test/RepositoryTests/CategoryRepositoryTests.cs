namespace ECommerce.Catalog.Test.RepositoryTests;

public class CategoryRepositoryTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CategoryRepository _categoryRepository;

    public CategoryRepositoryTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new MainDbContext(options);
        _loggerMock = new Mock<ILogger>();
        _categoryRepository = new CategoryRepository(_dbContext, _loggerMock.Object);
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
        Category category = new() { Name = "Laptops", Slug = "laptops" };

        // Act
        Category created = await _categoryRepository.AddAsync(category, CancellationToken.None);

        // Assert
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(1, _dbContext.Categories.Count());

        EntityEntry<Category> trackedEntity = _dbContext.ChangeTracker.Entries<Category>().Single();
        Assert.Equal(EntityState.Unchanged, trackedEntity.State); // Unchanged because GetAsync reloads it
    }

    [Fact]
    public async Task Test_DeleteAsync()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Category category = new() { Id = id, Name = "Laptops", Slug = "laptops", Version = version };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act
        Category? deleted = await _categoryRepository.DeleteAsync(id, version, CancellationToken.None);

        // Assert
        Assert.NotNull(deleted);
        Assert.Equal(0, _dbContext.Categories.Count());
    }

    [Fact]
    public async Task Test_UpdateAsync()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Category category = new() { Id = id, Name = "Laptops", Slug = "laptops", Version = version };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Simulate fetching and mutating in the service
        Category trackedCategory = (await _categoryRepository.GetAsync(id, CancellationToken.None))!;
        trackedCategory.Name = "Notebooks";

        // Act
        await _categoryRepository.UpdateAsync(trackedCategory, version, CancellationToken.None);

        // Assert
        Category? dbRecord = await _dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, CancellationToken.None);
        Assert.NotNull(dbRecord);
        Assert.Equal("Notebooks", dbRecord.Name);
    }

    [Fact]
    public async Task Test_GetAllAsync()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            _dbContext.Categories.Add(new Category { Id = Guid.NewGuid(), Name = $"Cat {i}", Slug = $"cat-{i}" });
        }
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act
        PagedResult<Category> result = await _categoryRepository.GetAllAsync(1, 10, CancellationToken.None);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.Empty(_dbContext.ChangeTracker.Entries()); // Because of AsNoTracking
    }
}