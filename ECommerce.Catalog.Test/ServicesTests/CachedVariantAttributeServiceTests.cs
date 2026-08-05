namespace ECommerce.Catalog.Test.ServicesTests;

public class CachedVariantAttributeServiceTests
{
    private readonly Mock<IVariantAttributeService> _innerServiceMock;
    private readonly TestHybridCache _cacheFake;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CachedVariantAttributeService _cachedService;
    private static readonly CompositeFormat CategoryAttributeFormat = CompositeFormat.Parse(CacheKeys.AttributeKey);
    private static readonly CompositeFormat PaginatedAttributesFormat = CompositeFormat.Parse(CacheKeys.PaginatedAttributes);

    public CachedVariantAttributeServiceTests()
    {
        _innerServiceMock = new Mock<IVariantAttributeService>();
        _cacheFake = new TestHybridCache();
        _loggerMock = new Mock<ILogger>();

        _cachedService = new CachedVariantAttributeService(_innerServiceMock.Object, _cacheFake, _loggerMock.Object);
    }

    // --- CreateAsync Tests ---

    [Fact]
    public async Task CreateAsyncReturnsSuccessAndUpdatesCache()
    {
        // Arrange
        CreateVariantAttributeDto dto = new("Color", "Red");
        VariantAttributeDto createdDto = new(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Color", "Red");
        ApiResponse<VariantAttributeDto> innerResponse = ApiResponse<VariantAttributeDto>.Success(createdDto);

        _innerServiceMock.Setup(x => x.CreateAsync("testuser", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(innerResponse);

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Contains(string.Format(null, CategoryAttributeFormat, createdDto.Id), _cacheFake.SetKeys);
        Assert.Contains(CacheKeys.AllAttributesKey, _cacheFake.RemovedTags);
    }

    [Fact]
    public async Task CreateAsyncReturnsFailureAndSkipsCacheWhenInnerFails()
    {
        // Arrange
        CreateVariantAttributeDto dto = new("Color", "Red");
        ApiResponse<VariantAttributeDto> innerResponse = ApiResponse<VariantAttributeDto>.Failure("DB Error");

        _innerServiceMock.Setup(x => x.CreateAsync("testuser", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(innerResponse);

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Empty(_cacheFake.SetKeys);
    }

    [Fact]
    public async Task CreateAsyncReturnsSuccessButLogsWarningWhenCacheFails()
    {
        // Arrange
        CreateVariantAttributeDto dto = new("Color", "Red");
        VariantAttributeDto createdDto = new(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Color", "Red");

        _innerServiceMock.Setup(x => x.CreateAsync("testuser", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttributeDto>.Success(createdDto));

        _cacheFake.ThrowOnSet = true; // Simulate Redis Failure

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.CreateAsync("testuser", dto, CancellationToken.None);

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
        VariantAttributeDto deletedDto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Color", "Red");

        _innerServiceMock.Setup(x => x.DeleteAsync("testuser", id, version, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttributeDto>.Success(deletedDto));

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.DeleteAsync("testuser", id, version, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Contains(string.Format(null, CategoryAttributeFormat, id), _cacheFake.RemovedKeys);
        Assert.Contains(CacheKeys.AllAttributesKey, _cacheFake.RemovedTags);
    }

    [Fact]
    public async Task DeleteAsyncReturnsFailureAndSkipsCacheWhenInnerFails()
    {
        // Arrange
        _innerServiceMock.Setup(x => x.DeleteAsync("testuser", It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttributeDto>.NotFound("Not found"));

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Empty(_cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task DeleteAsyncReturnsSuccessButLogsWarningWhenCacheFails()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        VariantAttributeDto deletedDto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Color", "Red");

        _innerServiceMock.Setup(x => x.DeleteAsync("testuser", id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttributeDto>.Success(deletedDto));

        _cacheFake.ThrowOnRemove = true; // Simulate Redis Failure

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.DeleteAsync("testuser", id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        _loggerMock.Verify(x => x.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Failed to update cache")), It.IsAny<Guid>()), Times.Once);
    }

    // --- GetAllAsync Tests ---

    [Fact]
    public async Task GetAllAsyncReturnsSuccessAndDoesNotEvict()
    {
        // Arrange
        PagedResult<VariantAttributeDto> pagedResult = new([], 0, 1, 100, 0);
        ApiResponse<PagedResult<VariantAttributeDto>> successResponse = ApiResponse<PagedResult<VariantAttributeDto>>.Success(pagedResult);

        _innerServiceMock.Setup(x => x.GetAllAsync("testuser", 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        ApiResponse<PagedResult<VariantAttributeDto>> response = await _cachedService.GetAllAsync("testuser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Empty(_cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task GetAllAsyncEvictsCacheWhenFactoryReturnsError()
    {
        // Arrange
        string expectedCacheKey = string.Format(null, PaginatedAttributesFormat, 1, 100);
        ApiResponse<PagedResult<VariantAttributeDto>> errorResponse = ApiResponse<PagedResult<VariantAttributeDto>>.Failure("Database error");

        _innerServiceMock.Setup(x => x.GetAllAsync("testuser", 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        // Act
        ApiResponse<PagedResult<VariantAttributeDto>> response = await _cachedService.GetAllAsync("testuser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains(expectedCacheKey, _cacheFake.RemovedKeys); // Negative caching guard activated!
    }

    [Fact]
    public async Task GetAllAsyncLogsWarningWhenEvictionFails()
    {
        // Arrange
        ApiResponse<PagedResult<VariantAttributeDto>> errorResponse = ApiResponse<PagedResult<VariantAttributeDto>>.Failure("Database error");

        _innerServiceMock.Setup(x => x.GetAllAsync("testuser", 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        _cacheFake.ThrowOnRemove = true; // Simulate Redis down

        // Act
        ApiResponse<PagedResult<VariantAttributeDto>> response = await _cachedService.GetAllAsync("testuser", 1, 100, CancellationToken.None);

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
        VariantAttributeDto dto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Color", "Red");

        _innerServiceMock.Setup(x => x.GetAsync("testuser", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttributeDto>.Success(dto));

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.GetAsync("testuser", id, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Empty(_cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task GetAsyncEvictsCacheWhenFactoryReturnsError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string expectedCacheKey = string.Format(null, CategoryAttributeFormat, id);

        _innerServiceMock.Setup(x => x.GetAsync("testuser", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttributeDto>.NotFound("Not found"));

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.GetAsync("testuser", id, CancellationToken.None);

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
        UpdateVariantAttributeDto dto = new("Color", "New Blue");
        VariantAttributeDto updatedDto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Color", "New Blue");

        _innerServiceMock.Setup(x => x.UpdateAsync("testuser", id, version, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttributeDto>.Success(updatedDto));

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.UpdateAsync("testuser", id, version, dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Contains(string.Format(null, CategoryAttributeFormat, id), _cacheFake.SetKeys);
        Assert.Contains(CacheKeys.AllAttributesKey, _cacheFake.RemovedTags);
    }

    [Fact]
    public async Task UpdateAsyncReturnsFailureAndSkipsCacheWhenInnerFails()
    {
        // Arrange
        UpdateVariantAttributeDto dto = new("Color", "New Blue");

        _innerServiceMock.Setup(x => x.UpdateAsync("testuser", It.IsAny<Guid>(), It.IsAny<Guid>(), dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VariantAttributeDto>.Conflict("Conflict"));

        // Act
        ApiResponse<VariantAttributeDto> response = await _cachedService.UpdateAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Conflict, response.Code);
        Assert.Empty(_cacheFake.SetKeys);
    }
}