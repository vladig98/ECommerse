namespace ECommerce.Catalog.Test.ServicesTests;

public class ProductServiceTests
{
    private readonly Mock<IProductsRepository> _repositoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductsRepository>();
        _loggerMock = new Mock<ILogger>();
        _service = new ProductService(_repositoryMock.Object, _loggerMock.Object);
    }

    // --- CreateAsync Tests ---

    [Fact]
    public async Task CreateAsyncReturnsSuccessWhenValid()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        CreateProductDto dto = new("Laptop", "laptop", "A fast laptop", "TechBrand", true, categoryId, [], []);

        Product createdProduct = new()
        {
            Id = Guid.NewGuid(),
            Title = "Laptop",
            Slug = "laptop",
            Category = new Category { Id = categoryId, Name = "Electronics" }
        };

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        ApiResponse<ProductDto> response = await _service.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(createdProduct.Id, response.Data.Id);
        Assert.Null(response.Error);
    }

    [Fact]
    public async Task CreateAsyncReturnsFailureOnDbUpdateException()
    {
        // Arrange
        CreateProductDto dto = new("Laptop", "laptop", null, null, true, Guid.NewGuid(), [], []);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Constraint violation"));

        // Act
        ApiResponse<ProductDto> response = await _service.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Null(response.Data);
        Assert.Contains("database constraint was violated", response.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAsyncReturnsFailureOnGenericException()
    {
        // Arrange
        CreateProductDto dto = new("Laptop", "laptop", null, null, true, Guid.NewGuid(), [], []);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Catastrophic failure"));

        // Act
        ApiResponse<ProductDto> response = await _service.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("unexpected error occurred", response.Error, StringComparison.Ordinal);
    }

    // --- DeleteAsync Tests ---

    [Fact]
    public async Task DeleteAsyncReturnsSuccessWhenValid()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        Product deletedProduct = new() { Id = id, Title = "Laptop", Slug = "laptop" };

        _repositoryMock.Setup(x => x.DeleteAsync(id, version, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedProduct);

        // Act
        ApiResponse<ProductDto> response = await _service.DeleteAsync("testuser", id, version, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(id, response.Data.Id);
    }

    [Fact]
    public async Task DeleteAsyncReturnsNotFoundWhenEntityMissing()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        ApiResponse<ProductDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Contains("could not be found", response.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteAsyncReturnsConflictOnConcurrencyException()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

        // Act
        ApiResponse<ProductDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Conflict, response.Code);
        Assert.Contains("modified by another process", response.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteAsyncReturnsFailureOnDbUpdateException()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Foreign key violation"));

        // Act
        ApiResponse<ProductDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("currently referenced by other records", response.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteAsyncReturnsFailureOnGenericException()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Generic error"));

        // Act
        ApiResponse<ProductDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("unexpected error occurred", response.Error, StringComparison.Ordinal);
    }

    // --- GetAllAsync Tests ---

    [Fact]
    public async Task GetAllAsyncReturnsSuccessWithPagedData()
    {
        // Arrange
        List<Product> items = [new() { Id = Guid.NewGuid(), Title = "Prod 1", Slug = "prod-1" }];
        PagedResult<Product> pagedResult = new(items, 1, 1, 100, 1);

        _repositoryMock.Setup(x => x.GetAllAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        ApiResponse<PagedResult<ProductDto>> response = await _service.GetAllAsync("testuser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data.Items);
        Assert.Equal(1, response.Data.TotalCount);
    }

    [Fact]
    public async Task GetAllAsyncReturnsFailureOnException()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Database down"));

        // Act
        ApiResponse<PagedResult<ProductDto>> response = await _service.GetAllAsync("testuser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("unexpected error occurred", response.Error, StringComparison.Ordinal);
    }

    // --- GetAsync Tests ---

    [Fact]
    public async Task GetAsyncReturnsSuccessWhenFound()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Product product = new() { Id = id, Title = "Laptop", Slug = "laptop" };

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        ApiResponse<ProductDto> response = await _service.GetAsync("testuser", id, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(id, response.Data.Id);
    }

    [Fact]
    public async Task GetAsyncReturnsNotFoundWhenMissing()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        ApiResponse<ProductDto> response = await _service.GetAsync("testuser", Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
    }

    [Fact]
    public async Task GetAsyncReturnsFailureOnException()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Timeout"));

        // Act
        ApiResponse<ProductDto> response = await _service.GetAsync("testuser", Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
    }

    // --- UpdateAsync Tests ---

    [Fact]
    public async Task UpdateAsyncReturnsSuccessWhenValid()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        Product existingProduct = new() { Id = id, Title = "Old Title", Slug = "old-slug", Category = new Category { Id = categoryId } };

        UpdateProductDto dto = new("New Title", "new-slug", "Desc", "Brand", true, categoryId, [], []);

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(x => x.UpdateAsync(existingProduct, version, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        ApiResponse<ProductDto> response = await _service.UpdateAsync("testuser", id, version, dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal("New Title", response.Data.Title); // Verifies the entity was mutated correctly before returning
    }

    [Fact]
    public async Task UpdateAsyncReturnsNotFoundWhenMissing()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        UpdateProductDto dto = new("New Title", "new-slug", "Desc", "Brand", true, Guid.NewGuid(), [], []);

        // Act
        ApiResponse<ProductDto> response = await _service.UpdateAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsyncReturnsConflictOnConcurrencyException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Product existingProduct = new() { Id = id };
        UpdateProductDto dto = new("New Title", "new-slug", "Desc", "Brand", true, Guid.NewGuid(), [], []);

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(x => x.UpdateAsync(existingProduct, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Conflict"));

        // Act
        ApiResponse<ProductDto> response = await _service.UpdateAsync("testuser", id, Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Conflict, response.Code);
    }

    [Fact]
    public async Task UpdateAsyncReturnsFailureOnDbUpdateException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Product existingProduct = new() { Id = id };
        UpdateProductDto dto = new("New Title", "new-slug", "Desc", "Brand", true, Guid.NewGuid(), [], []);

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(x => x.UpdateAsync(existingProduct, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Duplicate slug"));

        // Act
        ApiResponse<ProductDto> response = await _service.UpdateAsync("testuser", id, Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("database constraint was violated", response.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAsyncReturnsFailureOnGenericException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Product existingProduct = new() { Id = id };
        UpdateProductDto dto = new("New Title", "new-slug", "Desc", "Brand", true, Guid.NewGuid(), [], []);

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(x => x.UpdateAsync(existingProduct, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Random failure"));

        // Act
        ApiResponse<ProductDto> response = await _service.UpdateAsync("testuser", id, Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("unexpected error occurred", response.Error, StringComparison.Ordinal);
    }
}