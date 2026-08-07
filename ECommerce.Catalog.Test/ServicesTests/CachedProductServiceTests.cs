namespace ECommerce.Catalog.Test.ServicesTests;

public class CachedProductServiceTests
{
    private readonly Mock<IProductService> _innerServiceMock;
    private readonly TestHybridCache _cacheFake;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CachedProductService _cachedService;
    private static readonly CompositeFormat ProductKeyFormat = CompositeFormat.Parse(CacheKeys.ProductKey);
    private static readonly CompositeFormat PaginatedProductsFormat = CompositeFormat.Parse(CacheKeys.PaginatedProducts);

    public CachedProductServiceTests()
    {
        _innerServiceMock = new Mock<IProductService>();
        _cacheFake = new TestHybridCache();
        _loggerMock = new Mock<ILogger>();

        _cachedService = new CachedProductService(_innerServiceMock.Object, _cacheFake, _loggerMock.Object);
    }

    // --- CreateAsync Tests ---

    [Fact]
    public async Task CreateAsyncReturnsSuccessAndUpdatesCache()
    {
        // Arrange
        CreateProductDto dto = new("Laptop", "laptop", "Fast", "Brand", true, Guid.NewGuid(), [], []);
        ProductDto createdDto = new(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptop", "laptop", "Fast", "Brand", true, null, [], [], null!);
        ApiResponse<ProductDto> innerResponse = ApiResponse<ProductDto>.Success(createdDto);

        _innerServiceMock.Setup(x => x.CreateAsync("testuser", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(innerResponse);

        // Act
        ApiResponse<ProductDto> response = await _cachedService.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Contains(string.Format(null, ProductKeyFormat, createdDto.Id), _cacheFake.SetKeys);
        Assert.Contains(CacheKeys.AllProductsKey, _cacheFake.RemovedTags);
    }

    [Fact]
    public async Task CreateAsyncReturnsFailureAndSkipsCacheWhenInnerFails()
    {
        // Arrange
        CreateProductDto dto = new("Laptop", "laptop", null, null, true, Guid.NewGuid(), [], []);
        ApiResponse<ProductDto> innerResponse = ApiResponse<ProductDto>.Failure("DB Error");

        _innerServiceMock.Setup(x => x.CreateAsync("testuser", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(innerResponse);

        // Act
        ApiResponse<ProductDto> response = await _cachedService.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Empty(_cacheFake.SetKeys);
    }

    [Fact]
    public async Task CreateAsyncReturnsSuccessButLogsWarningWhenCacheFails()
    {
        // Arrange
        CreateProductDto dto = new("Laptop", "laptop", null, null, true, Guid.NewGuid(), [], []);
        ProductDto createdDto = new(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptop", "laptop", null, null, true, null, [], [], null!);

        _innerServiceMock.Setup(x => x.CreateAsync("testuser", dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductDto>.Success(createdDto));

        _cacheFake.ThrowOnSet = true; // Simulate a Redis failure

        // Act
        ApiResponse<ProductDto> response = await _cachedService.CreateAsync("testuser", dto, CancellationToken.None);

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
        ProductDto deletedDto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptop", "laptop", null, null, true, null, [], [], null!);

        _innerServiceMock.Setup(x => x.DeleteAsync("testuser", id, version, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductDto>.Success(deletedDto));

        // Act
        ApiResponse<ProductDto> response = await _cachedService.DeleteAsync("testuser", id, version, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Contains(string.Format(null, ProductKeyFormat, id), _cacheFake.RemovedKeys);
        Assert.Contains(CacheKeys.AllProductsKey, _cacheFake.RemovedTags);
    }

    [Fact]
    public async Task DeleteAsyncReturnsFailureAndSkipsCacheWhenInnerFails()
    {
        // Arrange
        _innerServiceMock.Setup(x => x.DeleteAsync("testuser", It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductDto>.NotFound("Not found"));

        // Act
        ApiResponse<ProductDto> response = await _cachedService.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Empty(_cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task DeleteAsyncReturnsSuccessButLogsWarningWhenCacheFails()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        ProductDto deletedDto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptop", "laptop", null, null, true, null, [], [], null!);

        _innerServiceMock.Setup(x => x.DeleteAsync("testuser", id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductDto>.Success(deletedDto));

        _cacheFake.ThrowOnRemove = true; // Simulate Redis down

        // Act
        ApiResponse<ProductDto> response = await _cachedService.DeleteAsync("testuser", id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        _loggerMock.Verify(x => x.Warning(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Failed to update cache")), It.IsAny<Guid>()), Times.Once);
    }

    // --- GetAllAsync Tests ---

    [Fact]
    public async Task GetAllAsyncReturnsSuccessAndDoesNotEvict()
    {
        // Arrange
        PagedResult<ProductDto> pagedResult = new([], 0, 1, 100, 0);
        ApiResponse<PagedResult<ProductDto>> successResponse = ApiResponse<PagedResult<ProductDto>>.Success(pagedResult);

        _innerServiceMock.Setup(x => x.GetAllAsync("testuser", 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResponse);

        // Act
        ApiResponse<PagedResult<ProductDto>> response = await _cachedService.GetAllAsync("testuser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Empty(_cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task GetAllAsyncEvictsCacheWhenFactoryReturnsError()
    {
        // Arrange
        string expectedCacheKey = string.Format(null, PaginatedProductsFormat, 1, 100);
        ApiResponse<PagedResult<ProductDto>> errorResponse = ApiResponse<PagedResult<ProductDto>>.Failure("Database error");

        _innerServiceMock.Setup(x => x.GetAllAsync("testuser", 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        // Act
        ApiResponse<PagedResult<ProductDto>> response = await _cachedService.GetAllAsync("testuser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains(expectedCacheKey, _cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task GetAllAsyncLogsWarningWhenEvictionFails()
    {
        // Arrange
        ApiResponse<PagedResult<ProductDto>> errorResponse = ApiResponse<PagedResult<ProductDto>>.Failure("Database error");

        _innerServiceMock.Setup(x => x.GetAllAsync("testuser", 1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResponse);

        _cacheFake.ThrowOnRemove = true;

        // Act
        ApiResponse<PagedResult<ProductDto>> response = await _cachedService.GetAllAsync("testuser", 1, 100, CancellationToken.None);

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
        ProductDto dto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptop", "laptop", null, null, true, null, [], [], null!);

        _innerServiceMock.Setup(x => x.GetAsync("testuser", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductDto>.Success(dto));

        // Act
        ApiResponse<ProductDto> response = await _cachedService.GetAsync("testuser", id, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Empty(_cacheFake.RemovedKeys);
    }

    [Fact]
    public async Task GetAsyncEvictsCacheWhenFactoryReturnsError()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string expectedCacheKey = string.Format(null, ProductKeyFormat, id);

        _innerServiceMock.Setup(x => x.GetAsync("testuser", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductDto>.NotFound("Not found"));

        // Act
        ApiResponse<ProductDto> response = await _cachedService.GetAsync("testuser", id, CancellationToken.None);

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
        UpdateProductDto dto = new("Laptop", "laptop", null, null, true, Guid.NewGuid(), [], []);
        ProductDto updatedDto = new(id, DateTime.UtcNow, DateTime.UtcNow, Guid.NewGuid(), "Laptop", "laptop", null, null, true, null, [], [], null!);

        _innerServiceMock.Setup(x => x.UpdateAsync("testuser", id, version, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductDto>.Success(updatedDto));

        // Act
        ApiResponse<ProductDto> response = await _cachedService.UpdateAsync("testuser", id, version, dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Contains(string.Format(null, ProductKeyFormat, id), _cacheFake.SetKeys);
        Assert.Contains(CacheKeys.AllProductsKey, _cacheFake.RemovedTags);
    }

    [Fact]
    public async Task UpdateAsyncReturnsFailureAndSkipsCacheWhenInnerFails()
    {
        // Arrange
        UpdateProductDto dto = new("Laptop", "laptop", null, null, true, Guid.NewGuid(), [], []);

        _innerServiceMock.Setup(x => x.UpdateAsync("testuser", It.IsAny<Guid>(), It.IsAny<Guid>(), dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductDto>.Conflict("Conflict"));

        // Act
        ApiResponse<ProductDto> response = await _cachedService.UpdateAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Conflict, response.Code);
        Assert.Empty(_cacheFake.SetKeys);
    }
}