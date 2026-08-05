namespace ECommerce.Catalog.Test.ServicesTests;

public class CachedCategoryServiceTests
{
    private readonly Mock<ICategoryService> _innerServiceMock;
    private readonly TestHybridCache _cacheFake;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CachedCategoryService _cachedService;
    private static readonly CompositeFormat CategoryKeyFormat = CompositeFormat.Parse(CacheKeys.CategoryKey);
    private static readonly CompositeFormat PaginatedCategoriesFormat = CompositeFormat.Parse(CacheKeys.PaginatedCategories);

    public CachedCategoryServiceTests()
    {
        _innerServiceMock = new Mock<ICategoryService>();
        _cacheFake = new TestHybridCache();
        _loggerMock = new Mock<ILogger>();

        _cachedService = new CachedCategoryService(_innerServiceMock.Object, _cacheFake, _loggerMock.Object);
    }

    // --- CreateAsync Tests ---

    [Fact]
    public async Task CreateAsyncReturnsSuccessAndUpdatesCache()
    {
        // Arrange
        CreateCategoryDto dto = new("Laptops", "laptops", null);
        CategoryDto createdDto = new(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptops", "laptops", null, []);
        ApiResponse<CategoryDto> innerResponse = ApiResponse<CategoryDto>.Success(createdDto);

        _innerServiceMock.Setup(x => x.CreateAsync("testuser", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(innerResponse);

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Contains(string.Format(null, CategoryKeyFormat, createdDto.Id), _cacheFake.SetKeys);
        Assert.Contains(CacheKeys.AllCategoriesKey, _cacheFake.RemovedTags);
    }

    [Fact]
    public async Task CreateAsyncReturnsFailureAndSkipsCacheWhenInnerFails()
    {
        // Arrange
        CreateCategoryDto dto = new("Laptops", "laptops", null);
        ApiResponse<CategoryDto> innerResponse = ApiResponse<CategoryDto>.Failure("DB Error");

        _innerServiceMock.Setup(x => x.CreateAsync("testuser", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(innerResponse);

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Empty(_cacheFake.SetKeys);
    }

    [Fact]
    public async Task CreateAsyncReturnsSuccessButLogsWarningWhenCacheFails()
    {
        // Arrange
        CreateCategoryDto dto = new("Laptops", "laptops", null);
        CategoryDto createdDto = new(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptops", "laptops", null, []);

        _innerServiceMock.Setup(x => x.CreateAsync("testuser", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<CategoryDto>.Success(createdDto));

        _cacheFake.ThrowOnSet = true;

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        _loggerMock.Verify(x => x.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Failed to update cache")), It.IsAny<Guid>()), Times.Once);
    }

    // --- DeleteAsync Tests ---

    [Fact]
    public async Task DeleteAsyncReturnsSuccessAndEvictsCache()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        CategoryDto deletedDto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptops", "laptops", null, []);

        _innerServiceMock.Setup(x => x.DeleteAsync("testuser", id, version, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<CategoryDto>.Success(deletedDto));

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.DeleteAsync("testuser", id, version, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Contains(string.Format(null, CategoryKeyFormat, id), _cacheFake.RemovedKeys);
        Assert.Contains(CacheKeys.AllCategoriesKey, _cacheFake.RemovedTags);
    }

    [Fact]
    public async Task DeleteAsyncReturnsFailureAndSkipsCacheWhenInnerFails()
    {
        // Arrange
        _innerServiceMock.Setup(x => x.DeleteAsync("testuser", It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<CategoryDto>.NotFound("Not found"));

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Empty(_cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task DeleteAsyncReturnsSuccessButLogsWarningWhenCacheFails()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        CategoryDto deletedDto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptops", "laptops", null, []);

        _innerServiceMock.Setup(x => x.DeleteAsync("testuser", id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<CategoryDto>.Success(deletedDto));

        _cacheFake.ThrowOnRemove = true;

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.DeleteAsync("testuser", id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        _loggerMock.Verify(x => x.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Failed to update cache")), It.IsAny<Guid>()), Times.Once);
    }

    // --- GetAllAsync Tests ---

    [Fact]
    public async Task GetAllAsyncReturnsSuccessAndDoesNotEvict()
    {
        // Arrange
        PagedResult<CategoryDto> pagedResult = new([], 0, 1, 100, 0);
        ApiResponse<PagedResult<CategoryDto>> successResponse = ApiResponse<PagedResult<CategoryDto>>.Success(pagedResult);

        _innerServiceMock.Setup(x => x.GetAllAsync("testuser", 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        ApiResponse<PagedResult<CategoryDto>> response = await _cachedService.GetAllAsync("testuser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Empty(_cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task GetAllAsyncEvictsCacheWhenFactoryReturnsError()
    {
        // Arrange
        string expectedCacheKey = string.Format(null, PaginatedCategoriesFormat, 1, 100);
        ApiResponse<PagedResult<CategoryDto>> errorResponse = ApiResponse<PagedResult<CategoryDto>>.Failure("Database error");

        _innerServiceMock.Setup(x => x.GetAllAsync("testuser", 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        // Act
        ApiResponse<PagedResult<CategoryDto>> response = await _cachedService.GetAllAsync("testuser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains(expectedCacheKey, _cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task GetAllAsyncLogsWarningWhenEvictionFails()
    {
        // Arrange
        ApiResponse<PagedResult<CategoryDto>> errorResponse = ApiResponse<PagedResult<CategoryDto>>.Failure("Database error");

        _innerServiceMock.Setup(x => x.GetAllAsync("testuser", 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        _cacheFake.ThrowOnRemove = true;

        // Act
        ApiResponse<PagedResult<CategoryDto>> response = await _cachedService.GetAllAsync("testuser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        _loggerMock.Verify(x => x.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Failed to evict failed response from cache for key")), It.IsAny<string>()), Times.Once);
    }

    // --- GetAsync Tests ---

    [Fact]
    public async Task GetAsyncReturnsSuccessAndDoesNotEvict()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        CategoryDto dto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptops", "laptops", null, []);

        _innerServiceMock.Setup(x => x.GetAsync("testuser", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<CategoryDto>.Success(dto));

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.GetAsync("testuser", id, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Empty(_cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task GetAsyncEvictsCacheWhenFactoryReturnsError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string expectedCacheKey = string.Format(null, CategoryKeyFormat, id);

        _innerServiceMock.Setup(x => x.GetAsync("testuser", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<CategoryDto>.NotFound("Not found"));

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.GetAsync("testuser", id, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Contains(expectedCacheKey, _cacheFake.RemovedKeys);
    }

    // --- UpdateAsync Tests ---

    [Fact]
    public async Task UpdateAsyncReturnsSuccessAndUpdatesCache()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        UpdateCategoryDto dto = new("Laptops", "laptops", null);
        CategoryDto updatedDto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptops", "laptops", null, []);

        _innerServiceMock.Setup(x => x.UpdateAsync("testuser", id, version, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<CategoryDto>.Success(updatedDto));

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.UpdateAsync("testuser", id, version, dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Contains(string.Format(null, CategoryKeyFormat, id), _cacheFake.SetKeys);
        Assert.Contains(CacheKeys.AllCategoriesKey, _cacheFake.RemovedTags);
    }

    [Fact]
    public async Task UpdateAsyncReturnsFailureAndSkipsCacheWhenInnerFails()
    {
        // Arrange
        UpdateCategoryDto dto = new("Laptops", "laptops", null);

        _innerServiceMock.Setup(x => x.UpdateAsync("testuser", It.IsAny<Guid>(), It.IsAny<Guid>(), dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<CategoryDto>.Conflict("Conflict"));

        // Act
        ApiResponse<CategoryDto> response = await _cachedService.UpdateAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Conflict, response.Code);
        Assert.Empty(_cacheFake.SetKeys);
    }
}