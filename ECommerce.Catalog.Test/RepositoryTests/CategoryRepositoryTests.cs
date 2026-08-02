using Serilog.Events;

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

    #region Happy Path Tests

    [Fact]
    public void Test_Create()
    {
        // Arrange
        Guid fakeParentId = Guid.NewGuid();
        CreateCategoryDto dto = new("Laptops", "laptops", fakeParentId, []);

        // Act
        ApiResponse<Category> response = _categoryRepository.Create("TestUser", dto);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: Unit of Work State (Not saved to DB yet)
        Assert.Equal(0, _dbContext.Categories.Count());

        // Assert 3: Change Tracker State
        EntityEntry<Category> trackedEntity = _dbContext.ChangeTracker.Entries<Category>().Single();
        Assert.Equal(EntityState.Added, trackedEntity.State);

        // Assert 4: Relationship Test
        Category category = response.Data;
        Assert.Null(category.ParentCategoryId);
        Assert.Null(category.ParentCategory);
    }

    [Fact]
    public async Task Test_Delete()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Category category = new() { Id = id, Name = "Laptops", Slug = "laptops", Version = version };

        _dbContext.Categories.Add(category);
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear(); // Detach seeded entity for clean request scope

        // Act
        ApiResponse<Category> response = await _categoryRepository.DeleteAsync("TestUser", id, version, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: Unit of Work State
        Assert.Equal(1, _dbContext.Categories.Count());

        // Assert 3: Change Tracker State
        EntityEntry<Category> trackedEntity = _dbContext.ChangeTracker.Entries<Category>().Single();
        Assert.Equal(EntityState.Deleted, trackedEntity.State);

        // Assert 4: Entity Properties
        Category deletedCategory = response.Data;
        Assert.Equal(id, deletedCategory.Id);
        Assert.Equal("Laptops", deletedCategory.Name);
    }

    [Fact]
    public async Task Test_Update()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Category category = new() { Id = id, Name = "Laptops", Slug = "laptops", Version = version };

        _dbContext.Categories.Add(category);
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear(); // Detach seeded entity for clean request scope

        UpdateCategoryDto dto = new("Notebooks", "notebooks", null);

        // Act
        ApiResponse<Category> response = await _categoryRepository.UpdateAsync("TestUser", id, version, dto, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: Change Tracker State
        EntityEntry<Category> trackedEntity = _dbContext.ChangeTracker.Entries<Category>().Single();
        Assert.Equal(EntityState.Modified, trackedEntity.State);

        // Assert 3: Entity Mutations
        Category updatedCategory = response.Data;
        Assert.Equal(id, updatedCategory.Id);
        Assert.Equal(dto.Name, updatedCategory.Name);
        Assert.Equal(dto.Slug, updatedCategory.Slug);
    }

    [Fact]
    public async Task Test_Get()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Category category = new() { Id = id, Name = "Laptops", Slug = "laptops", Version = Guid.NewGuid() };

        _dbContext.Categories.Add(category);
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();

        // Act
        ApiResponse<Category> response = await _categoryRepository.GetAsync("TestUser", id, CancellationToken.None);

        // Assert
        Assert.True(string.IsNullOrEmpty(response.Error));
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        EntityEntry<Category> trackedEntity = _dbContext.ChangeTracker.Entries<Category>().Single();
        Assert.Equal(EntityState.Unchanged, trackedEntity.State);
    }

    [Fact]
    public async Task Test_Get_All()
    {
        // Arrange
        const int itemCount = 5;
        for (int i = 0; i < itemCount; i++)
        {
            _dbContext.Categories.Add(new Category { Id = Guid.NewGuid(), Name = $"Category {i}", Slug = $"category-{i}" });
        }
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();

        // Act
        ApiResponse<List<Category>> response = await _categoryRepository.GetAllAsync("TestUser", CancellationToken.None);

        // Assert
        Assert.True(string.IsNullOrEmpty(response.Error));
        Assert.NotNull(response.Data);
        Assert.Equal(itemCount, response.Data.Count);

        foreach (EntityEntry<Category> entry in _dbContext.ChangeTracker.Entries<Category>())
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
        }
    }

    #endregion

    #region NotFound Failure Tests

    [Fact]
    public async Task Test_Delete_NotFound()
    {
        // Act
        ApiResponse<Category> response = await _categoryRepository.DeleteAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(response.Error);
        Assert.Null(response.Data);
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Contains("could not be found", response.Error);
    }

    [Fact]
    public async Task Test_Update_NotFound()
    {
        // Arrange
        UpdateCategoryDto dto = new("Missing", "missing", null);

        // Act
        ApiResponse<Category> response = await _categoryRepository.UpdateAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.NotNull(response.Error);
        Assert.Null(response.Data);
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Contains("could not be found", response.Error);
    }

    [Fact]
    public async Task Test_Get_NotFound()
    {
        // Act
        ApiResponse<Category> response = await _categoryRepository.GetAsync("TestUser", Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.NotNull(response.Error);
        Assert.Null(response.Data);
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Contains("could not be found", response.Error);
    }

    #endregion

    #region Exception Handling Tests (Logger Failure Simulation)

    [Fact]
    public void Create_Returns_Generic_Failure_When_Logger_Throws()
    {
        // Arrange
        _loggerMock
    .Setup(x => x.IsEnabled(It.IsAny<LogEventLevel>()))
    .Throws(new InvalidOperationException("Simulated Logger Failure"));

        CreateCategoryDto dto = new("Laptops", "laptops", null, []);

        // Act
        ApiResponse<Category> response = _categoryRepository.Create("TestUser", dto);

        // Assert
        Assert.Null(response.Data);
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.NotNull(response.Error);
        Assert.Contains("unexpected error occurred", response.Error);

        // Verify error was logged by the catch block
        _loggerMock.Verify(
            x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object?[]>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Returns_Generic_Failure_When_Logger_Throws()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        _dbContext.Categories.Add(new Category { Id = id, Name = "Laptops", Slug = "laptops", Version = version });
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();

        _loggerMock
            .Setup(x => x.Information(It.IsAny<string>(), It.IsAny<object?[]>()))
            .Throws(new InvalidOperationException("Simulated Logger Failure"));

        // Act
        ApiResponse<Category> response = await _categoryRepository.DeleteAsync("TestUser", id, version, CancellationToken.None);

        // Assert
        Assert.Null(response.Data);
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.NotNull(response.Error);
        Assert.Contains("unexpected error occurred", response.Error);

        _loggerMock.Verify(
            x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object?[]>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Returns_Generic_Failure_When_Logger_Throws()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        _dbContext.Categories.Add(new Category { Id = id, Name = "Laptops", Slug = "laptops", Version = version });
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();

        _loggerMock
            .Setup(x => x.Information(It.IsAny<string>(), It.IsAny<object?[]>()))
            .Throws(new InvalidOperationException("Simulated Logger Failure"));

        UpdateCategoryDto dto = new("Notebooks", "notebooks", null);

        // Act
        ApiResponse<Category> response = await _categoryRepository.UpdateAsync("TestUser", id, version, dto, CancellationToken.None);

        // Assert
        Assert.Null(response.Data);
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.NotNull(response.Error);
        Assert.Contains("unexpected error occurred", response.Error);

        _loggerMock.Verify(
            x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object?[]>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_Returns_Generic_Failure_When_Logger_Throws()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _dbContext.Categories.Add(new Category { Id = id, Name = "Laptops", Slug = "laptops" });
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();

        _loggerMock
            .Setup(x => x.Debug(It.IsAny<string>(), It.IsAny<object?[]>()))
            .Throws(new InvalidOperationException("Simulated Logger Failure"));

        // Act
        ApiResponse<Category> response = await _categoryRepository.GetAsync("TestUser", id, CancellationToken.None);

        // Assert
        Assert.Null(response.Data);
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.NotNull(response.Error);
        Assert.Contains("unexpected error occurred", response.Error);

        _loggerMock.Verify(
            x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object?[]>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_Returns_Generic_Failure_When_Logger_Throws()
    {
        // Arrange
        _dbContext.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Laptops", Slug = "laptops" });
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();

        _loggerMock
            .Setup(x => x.Debug(It.IsAny<string>(), It.IsAny<object?[]>()))
            .Throws(new InvalidOperationException("Simulated Logger Failure"));

        // Act
        ApiResponse<List<Category>> response = await _categoryRepository.GetAllAsync("TestUser", CancellationToken.None);

        // Assert
        Assert.Null(response.Data);
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.NotNull(response.Error);
        Assert.Contains("unexpected error occurred", response.Error);

        _loggerMock.Verify(
            x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object?[]>()),
            Times.Once);
    }

    #endregion
}