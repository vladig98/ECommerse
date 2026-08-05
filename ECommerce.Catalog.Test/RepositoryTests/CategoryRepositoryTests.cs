namespace ECommerce.Catalog.Test.RepositoryTests;

public class CategoryRepositoryTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CategoryRepository _categoryRepository;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

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
    public async Task TestAddAsync()
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
        Assert.Equal(EntityState.Unchanged, trackedEntity.State);

        Assert.Equal(category.Id, created.Id);
        Assert.Equal(category.Name, created.Name);
        Assert.Equal(category.Slug, created.Slug);
        Assert.Equal(category.ParentCategoryId, created.ParentCategoryId);
    }

    [Fact]
    public async Task TestDeleteAsync()
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

        Assert.Equal(category.Id, deleted.Id);
        Assert.Equal(category.Name, deleted.Name);
        Assert.Equal(category.Slug, deleted.Slug);
        Assert.Equal(category.ParentCategoryId, deleted.ParentCategoryId);
    }

    [Fact]
    public async Task TestUpdateAsync()
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
    public async Task TestGetAllAsync()
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
        Assert.Empty(_dbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task TestGetAsyncReturnsCategoryWithRelations()
    {
        // Arrange
        Guid parentId = Guid.NewGuid();
        Guid childId = Guid.NewGuid();

        Category parent = new() { Id = parentId, Name = "Electronics", Slug = "electronics" };
        Category child = new() { Id = childId, Name = "Laptops", Slug = "laptops", ParentCategoryId = parentId };

        _dbContext.Categories.Add(parent);
        _dbContext.Categories.Add(child);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Clear the tracker to ensure we are actually querying the DB and testing the Include() statements
        _dbContext.ChangeTracker.Clear();

        // Act
        Category? result = await _categoryRepository.GetAsync(childId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(childId, result.Id);
        Assert.Equal("Laptops", result.Name);

        // Verify that GetAllRelatedEntities() successfully wired up the parent
        Assert.NotNull(result.ParentCategory);
        Assert.Equal(parentId, result.ParentCategory.Id);
        Assert.Equal("Electronics", result.ParentCategory.Name);
    }

    [Fact]
    public async Task TestGetAsyncReturnsNullIfNotFound()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        Category? result = await _categoryRepository.GetAsync(nonExistentId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}