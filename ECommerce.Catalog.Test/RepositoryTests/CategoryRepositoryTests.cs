namespace ECommerce.Catalog.Test.RepositoryTests;

public class CategoryRepositoryTests : IDisposable
{
    private readonly MainDbContext dbContext;
    private readonly CategoryRepository categoryRepository;
    private readonly ILogger logger;

    public CategoryRepositoryTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        dbContext = new MainDbContext(options);
        logger = new Mock<ILogger>().Object;
        categoryRepository = new CategoryRepository(dbContext, logger);
    }

    public void Dispose()
    {
        dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Test_Create()
    {
        Guid fakeParentId = Guid.NewGuid();
        CreateCategoryDto dto = new("Laptops", "laptops", fakeParentId, []);

        // Act
        ApiResponse<Category> response = categoryRepository.Create("TestUser", dto);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Categories.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<Category> trackedEntity = dbContext.ChangeTracker.Entries<Category>().Single();
        Assert.Equal(EntityState.Added, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        Category category = response.Data;
        Assert.Null(category.ParentCategoryId);
        Assert.Null(category.ParentCategory);
    }

    [Fact]
    public async Task Test_Delete()
    {
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();

        Category category = new()
        {
            Name = "Laptops",
            Slug = "laptops",
            Version = version,
            Id = id
        };

        dbContext.Categories.Add(category);
        dbContext.SaveChanges();

        // Act
        ApiResponse<Category> response = await categoryRepository.DeleteAsync("TestUser", id, version, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Categories.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<Category> trackedEntity = dbContext.ChangeTracker.Entries<Category>().Single();
        Assert.Equal(EntityState.Deleted, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        Category deletedCategory = response.Data;

        Assert.Equal(category.Id, deletedCategory.Id);
        Assert.Equal(category.Name, deletedCategory.Name);
        Assert.Equal(category.Slug, deletedCategory.Slug);

        Assert.Null(deletedCategory.ParentCategoryId);
        Assert.Null(deletedCategory.ParentCategory);
    }

    [Fact]
    public async Task Test_Update()
    {
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();

        Category category = new()
        {
            Name = "Laptops",
            Slug = "laptops",
            Version = version,
            Id = id
        };

        dbContext.Categories.Add(category);
        dbContext.SaveChanges();

        UpdateCategoryDto dto = new("name", "slug", null);

        // Act
        ApiResponse<Category> response = await categoryRepository.UpdateAsync("TestUser", id, version, dto, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Categories.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<Category> trackedEntity = dbContext.ChangeTracker.Entries<Category>().Single();
        Assert.Equal(EntityState.Modified, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        Category updatedCategory = response.Data;

        Assert.Equal(category.Id, updatedCategory.Id);
        Assert.Equal(dto.Name, updatedCategory.Name);
        Assert.Equal(dto.Slug, updatedCategory.Slug);

        Assert.Null(updatedCategory.ParentCategoryId);
        Assert.Null(updatedCategory.ParentCategory);
    }

    [Fact]
    public async Task Test_Get()
    {
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();

        Category category = new()
        {
            Name = "Laptops",
            Slug = "laptops",
            Version = version,
            Id = id
        };

        dbContext.Categories.Add(category);
        dbContext.SaveChanges();

        // Act
        ApiResponse<Category> response = await categoryRepository.GetAsync("TestUser", id, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Categories.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<Category> trackedEntity = dbContext.ChangeTracker.Entries<Category>().Single();
        Assert.Equal(EntityState.Unchanged, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        Category retrievedCategory = response.Data;

        Assert.Equal(category.Id, retrievedCategory.Id);
        Assert.Equal(category.Name, retrievedCategory.Name);
        Assert.Equal(category.Slug, retrievedCategory.Slug);

        Assert.Null(retrievedCategory.ParentCategoryId);
        Assert.Null(retrievedCategory.ParentCategory);
    }

    [Fact]
    public async Task Test_Get_All()
    {
        const int numberOfTests = 10;

        List<Guid> ids = [];
        for (int i = 0; i < numberOfTests; i++)
        {
            Guid id = Guid.NewGuid();
            Category category = new()
            {
                Name = "Laptops",
                Slug = "laptops",
                Version = Guid.NewGuid(),
                Id = id
            };

            ids.Add(id);
            dbContext.Categories.Add(category);
        }

        dbContext.SaveChanges();

        // Act
        ApiResponse<List<Category>> response = await categoryRepository.GetAllAsync("TestUser", CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Categories.Count();
        Assert.Equal(numberOfTests, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        List<EntityEntry<Category>> trackedEntities = [.. dbContext.ChangeTracker.Entries<Category>()];
        foreach (EntityEntry<Category> trackedEntity in trackedEntities)
        {
            Assert.Equal(EntityState.Unchanged, trackedEntity.State);
        }

        // Assert 4: THE RELATIONSHIP TEST
        List<Category> categories = response.Data;
        foreach (Category category in categories)
        {
            Assert.Contains(category.Id, ids);

            Assert.Null(category.ParentCategoryId);
            Assert.Null(category.ParentCategory);
        }        
    }

    [Fact]
    public async Task Test_Delete_NotFound()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid nonExistentId = Guid.NewGuid();

        // Act
        ApiResponse<Category> response = await categoryRepository.DeleteAsync("TestUser", nonExistentId, version, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the category does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Categories.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task Test_Update_NotFound()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid nonExistentId = Guid.NewGuid();

        UpdateCategoryDto dto = new("Missing Category", "missing-category", null);

        // Act
        ApiResponse<Category> response = await categoryRepository.UpdateAsync("TestUser", nonExistentId, version, dto, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the category does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Categories.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task Test_Get_NotFound()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        ApiResponse<Category> response = await categoryRepository.GetAsync("TestUser", nonExistentId, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the category does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Categories.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }
}