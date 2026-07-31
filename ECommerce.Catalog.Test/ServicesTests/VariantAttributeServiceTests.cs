namespace ECommerce.Catalog.Test.ServicesTests;

public class VariantAttributeServiceTests : IDisposable
{
    private readonly MainDbContext dbContext;
    private readonly Mock<IVariantAttributeRepository> mockRepository;
    private readonly TestHybridCache testCache;
    private readonly Mock<ILogger> mockLogger;
    private readonly VariantAttributeService variantAttributeService;

    public VariantAttributeServiceTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        dbContext = new MainDbContext(options);

        mockRepository = new Mock<IVariantAttributeRepository>();
        testCache = new TestHybridCache();
        mockLogger = new Mock<ILogger>();

        variantAttributeService = new VariantAttributeService(
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
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<CreateVariantAttributeDto>()))
            .Returns(ApiResponse<VariantAttribute>.Failure("Failed to prepare attribute"));

        CreateVariantAttributeDto dto = new("Color", "Space Gray");

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.CreateAsync("TestUser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Null(response.Data);
        Assert.Contains("Failed to prepare attribute", response.Error);

        // Verify cache and DB were untouched
        Assert.Empty(testCache.RemovedKeys);
        Assert.Empty(testCache.SetKeys);
        Assert.Equal(0, dbContext.VariantAttributes.Count());
    }

    [Fact]
    public async Task CreateAsync_SuccessPath_SetsCache_AndBustsGlobalList()
    {
        // Arrange
        Guid newId = Guid.NewGuid();
        VariantAttribute fakeAttribute = new() { Id = newId, Name = "Color", Value = "Space Gray" };

        mockRepository
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<CreateVariantAttributeDto>()))
            .Returns(ApiResponse<VariantAttribute>.Success(fakeAttribute));

        CreateVariantAttributeDto dto = new("Color", "Space Gray");

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.CreateAsync("TestUser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(newId, response.Data.Id);

        // Verify caching logic
        Assert.Contains(CacheKeys.AllAttributesKey, testCache.RemovedKeys);
        Assert.Contains(string.Format(CacheKeys.AttributeKey, newId), testCache.SetKeys);
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
            .ReturnsAsync(ApiResponse<VariantAttribute>.NotFound("Not found"));

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.DeleteAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

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
        VariantAttribute fakeAttribute = new() { Id = id, Name = "Color", Value = "Space Gray" };

        mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttribute>.Success(fakeAttribute));

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.DeleteAsync("TestUser", id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);

        Assert.Contains(string.Format(CacheKeys.AttributeKey, id), testCache.RemovedKeys);
        Assert.Contains(CacheKeys.AllAttributesKey, testCache.RemovedKeys);
    }

    // ==========================================
    // UPDATE ASYNC PATHS
    // ==========================================

    [Fact]
    public async Task UpdateAsync_RepositoryFails_ReturnsFailure()
    {
        // Arrange
        mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateVariantAttributeDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttribute>.NotFound("Missing attribute"));

        UpdateVariantAttributeDto dto = new(Guid.NewGuid(), Guid.NewGuid(), "Color", "Silver");

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.UpdateAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Null(response.Data);

        Assert.Empty(testCache.RemovedKeys);
        Assert.Empty(testCache.SetKeys);
    }

    [Fact]
    public async Task UpdateAsync_SuccessPath_UpdatesCache()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        VariantAttribute fakeAttribute = new() { Id = id, Name = "Color", Value = "Silver" };

        mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<string>(), id, It.IsAny<Guid>(), It.IsAny<UpdateVariantAttributeDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttribute>.Success(fakeAttribute));

        UpdateVariantAttributeDto dto = new(id, Guid.NewGuid(), "Color", "Silver");

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.UpdateAsync("TestUser", id, Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal("Silver", response.Data.Value);

        Assert.Contains(CacheKeys.AllAttributesKey, testCache.RemovedKeys);
        Assert.Contains(string.Format(CacheKeys.AttributeKey, id), testCache.SetKeys);
    }

    // ==========================================
    // GET & GETALL ASYNC PATHS
    // ==========================================

    [Fact]
    public async Task GetAllAsync_RepositoryFails_ReturnsEmptyList()
    {
        // Arrange
        mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<VariantAttribute>>.Failure("Database error"));

        // Act
        ApiResponse<List<VariantAttributeDto>> response = await variantAttributeService.GetAllAsync("TestUser", CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Empty(response.Data!);
    }

    [Fact]
    public async Task GetAllAsync_SuccessPath_ReturnsMappedDtos()
    {
        // Arrange
        List<VariantAttribute> fakeAttributes = [new VariantAttribute() { Id = Guid.NewGuid(), Name = "Size", Value = "Large" }];

        mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<VariantAttribute>>.Success(fakeAttributes));

        // Act
        ApiResponse<List<VariantAttributeDto>> response = await variantAttributeService.GetAllAsync("TestUser", CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Single(response.Data!);
        Assert.Equal("Size", response.Data![0].Name);
        Assert.Equal("Large", response.Data![0].Value);
    }

    [Fact]
    public async Task GetAsync_RepositoryFails_ReturnsNotFoundResponse()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        mockRepository
            .Setup(x => x.GetAsync(It.IsAny<string>(), id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttribute>.NotFound("DB Miss"));

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.GetAsync("TestUser", id, CancellationToken.None);

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
        VariantAttribute fakeAttribute = new() { Id = id, Name = "Size", Value = "Large" };

        mockRepository
            .Setup(x => x.GetAsync(It.IsAny<string>(), id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttribute>.Success(fakeAttribute));

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.GetAsync("TestUser", id, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal("Size", response.Data.Name);
        Assert.Equal("Large", response.Data.Value);
    }
}