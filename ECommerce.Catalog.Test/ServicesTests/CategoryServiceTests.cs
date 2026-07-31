namespace ECommerce.Catalog.Test.ServicesTests;

public class CategoryServiceTests : IDisposable
{
    private readonly MainDbContext dbContext;
    private readonly Mock<ICategoryRepository> mockRepository;
    private readonly TestHybridCache testCache;
    private readonly Mock<ILogger> mockLogger;
    private readonly CategoryService categoryService;

    public CategoryServiceTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        dbContext = new MainDbContext(options);

        mockRepository = new Mock<ICategoryRepository>();
        testCache = new TestHybridCache();
        mockLogger = new Mock<ILogger>();

        categoryService = new CategoryService(
            mockRepository.Object,
            dbContext,
            testCache,
            mockLogger.Object);
    }

    public void Dispose()
    {
        dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    // ==========================================
    // CREATE ASYNC PATHS
    // ==========================================

    [Fact]
    public async Task CreateAsync_RepositoryFails_ReturnsFailure_AndDoesNotCache()
    {
        // Arrange
        mockRepository
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<CreateCategoryDto>()))
            .Returns(ApiResponse<Category>.Failure("Failed to prepare category"));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.CreateAsync("TestUser", new CreateCategoryDto("Laptops", "laptops", null, []), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Null(response.Data);
        Assert.Contains("Failed to prepare category", response.Error);

        // Verify the cache was untouched
        Assert.Empty(testCache.RemovedKeys);
        Assert.Empty(testCache.SetKeys);

        Assert.Equal(0, dbContext.Categories.Count());
    }

    [Fact]
    public async Task CreateAsync_ParentCategoryNotFound_ReturnsFailure_AndDoesNotCache()
    {
        // Arrange
        Guid parentId = Guid.NewGuid();
        Category fakeChild = new() { Id = Guid.NewGuid(), Name = "Gaming", Slug = "gaming", ParentCategoryId = parentId };

        mockRepository
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<CreateCategoryDto>()))
            .Returns(ApiResponse<Category>.Success(fakeChild));

        mockRepository
            .Setup(x => x.GetAsync(It.IsAny<string>(), parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.NotFound("Parent missing"));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.CreateAsync("TestUser", new CreateCategoryDto("Gaming", "gaming", parentId, []), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Null(response.Data);
        Assert.Contains("Parent missing", response.Error);

        Assert.Empty(testCache.RemovedKeys);
        Assert.Empty(testCache.SetKeys);
    }

    [Fact]
    public async Task CreateAsync_SuccessPath_SetsCache_AndBustsGlobalList()
    {
        // Arrange
        Guid newId = Guid.NewGuid();
        Category fakeCategory = new() { Id = newId, Name = "Laptops", Slug = "laptops" };

        mockRepository
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<CreateCategoryDto>()))
            .Returns(ApiResponse<Category>.Success(fakeCategory));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.CreateAsync("TestUser", new CreateCategoryDto("Laptops", "laptops", null, []), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(newId, response.Data.Id);

        // Verify caching logic using the Fake Cache lists
        Assert.Contains(CacheKeys.AllCategoriesKey, testCache.RemovedKeys);
        Assert.Contains(string.Format(CacheKeys.CategoryKey, newId), testCache.SetKeys);
    }

    // ==========================================
    // DELETE ASYNC PATHS
    // ==========================================

    [Fact]
    public async Task DeleteAsync_RepositoryFails_ReturnsFailure_AndDoesNotCache()
    {
        // Arrange
        mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.NotFound("Not found"));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.DeleteAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Null(response.Data);

        Assert.Empty(testCache.RemovedKeys);
    }

    [Fact]
    public async Task DeleteAsync_SuccessPath_RemovesFromCaches()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Category fakeCategory = new() { Id = id, Name = "Laptops", Slug = "laptops" };

        mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.Success(fakeCategory));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.DeleteAsync("TestUser", id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);

        Assert.Contains(string.Format(CacheKeys.CategoryKey, id), testCache.RemovedKeys);
        Assert.Contains(CacheKeys.AllCategoriesKey, testCache.RemovedKeys);
    }

    // ==========================================
    // UPDATE ASYNC PATHS
    // ==========================================

    [Fact]
    public async Task UpdateAsync_RepositoryFails_ReturnsFailure()
    {
        // Arrange
        mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateCategoryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.NotFound("Missing category"));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.UpdateAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), new UpdateCategoryDto("Laptops", "laptops", null), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Null(response.Data);

        Assert.Empty(testCache.RemovedKeys);
        Assert.Empty(testCache.SetKeys);
    }

    [Fact]
    public async Task UpdateAsync_ParentCategoryNotFound_ReturnsFailure()
    {
        // Arrange
        Guid parentId = Guid.NewGuid();
        Category fakeCategory = new() { Id = Guid.NewGuid(), Name = "Gaming", Slug = "gaming" };

        mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateCategoryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.Success(fakeCategory));

        mockRepository
            .Setup(x => x.GetAsync(It.IsAny<string>(), parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.NotFound("Parent missing"));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.UpdateAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), new UpdateCategoryDto("Gaming", "gaming", parentId), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Null(response.Data);

        Assert.Empty(testCache.RemovedKeys);
        Assert.Empty(testCache.SetKeys);
    }

    [Fact]
    public async Task UpdateAsync_SuccessPath_WithNoParent_NullsParentAndUpdatesCache()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Category fakeCategory = new() { Id = id, Name = "Gaming", Slug = "gaming", ParentCategoryId = Guid.NewGuid(), ParentCategory = new Category() };

        mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<string>(), id, It.IsAny<Guid>(), It.IsAny<UpdateCategoryDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.Success(fakeCategory));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.UpdateAsync("TestUser", id, Guid.NewGuid(), new UpdateCategoryDto("Gaming", "gaming", null), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Null(response.Data.ParentCategory);

        Assert.Contains(CacheKeys.AllCategoriesKey, testCache.RemovedKeys);
        Assert.Contains(string.Format(CacheKeys.CategoryKey, id), testCache.SetKeys);
    }

    // ==========================================
    // GET & GETALL ASYNC PATHS (CACHE FACTORY TESTS)
    // ==========================================

    [Fact]
    public async Task GetAllAsync_RepositoryFails_ReturnsEmptyList()
    {
        // Arrange
        mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<Category>>.Failure("Database error"));

        // Act
        // Because of TestHybridCache, this will instantly bypass the cache logic and hit the mock repo!
        ApiResponse<List<CategoryDto>> response = await categoryService.GetAllAsync("TestUser", CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Empty(response.Data!);
    }

    [Fact]
    public async Task GetAllAsync_SuccessPath_ReturnsMappedDtos()
    {
        // Arrange
        List<Category> fakeCategories = [new Category() { Id = Guid.NewGuid(), Name = "Laptops", Slug = "laptops" }];

        mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<Category>>.Success(fakeCategories));

        // Act
        ApiResponse<List<CategoryDto>> response = await categoryService.GetAllAsync("TestUser", CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Single(response.Data!);
        Assert.Equal("Laptops", response.Data![0].Name);
    }

    [Fact]
    public async Task GetAsync_RepositoryFails_ReturnsNotFoundResponse()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        mockRepository
            .Setup(x => x.GetAsync(It.IsAny<string>(), id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.NotFound("DB Miss"));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.GetAsync("TestUser", id, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);
    }

    [Fact]
    public async Task GetAsync_SuccessPath_ReturnsMappedDto()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        Category fakeCategory = new() { Id = id, Name = "Laptops", Slug = "laptops" };

        mockRepository
            .Setup(x => x.GetAsync(It.IsAny<string>(), id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.Success(fakeCategory));

        // Act
        ApiResponse<CategoryDto> response = await categoryService.GetAsync("TestUser", id, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal("Laptops", response.Data.Name);
    }
}