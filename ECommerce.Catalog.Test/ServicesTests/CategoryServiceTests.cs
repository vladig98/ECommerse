namespace ECommerce.Catalog.Test.ServicesTests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _loggerMock = new Mock<ILogger>();
        _service = new CategoryService(_repositoryMock.Object, _loggerMock.Object);
    }

    // --- CreateAsync Tests ---

    [Fact]
    public async Task CreateAsyncReturnsSuccessWhenValid()
    {
        // Arrange
        CreateCategoryDto dto = new("Laptops", "laptops", null);
        Category createdCategory = new() { Id = Guid.NewGuid(), Name = "Laptops", Slug = "laptops" };

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        // Act
        ApiResponse<CategoryDto> response = await _service.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(createdCategory.Id, response.Data.Id);
        Assert.Null(response.Error);
    }

    [Fact]
    public async Task CreateAsyncReturnsFailureOnDbUpdateException()
    {
        // Arrange
        CreateCategoryDto dto = new("Laptops", "laptops", null);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Constraint violation"));

        // Act
        ApiResponse<CategoryDto> response = await _service.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Null(response.Data);
        Assert.Contains("database constraint was violated", response.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAsyncReturnsFailureOnGenericException()
    {
        // Arrange
        CreateCategoryDto dto = new("Laptops", "laptops", null);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Catastrophic failure"));

        // Act
        ApiResponse<CategoryDto> response = await _service.CreateAsync("testuser", dto, CancellationToken.None);

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
        Category deletedCategory = new() { Id = id, Name = "Laptops", Slug = "laptops" };

        _repositoryMock.Setup(x => x.DeleteAsync(id, version, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedCategory);

        // Act
        ApiResponse<CategoryDto> response = await _service.DeleteAsync("testuser", id, version, CancellationToken.None);

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
            .ReturnsAsync((Category?)null);

        // Act
        ApiResponse<CategoryDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

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
        ApiResponse<CategoryDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

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
        ApiResponse<CategoryDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("currently referenced by other records", response.Error, StringComparison.Ordinal);
    }

    // --- GetAllAsync Tests ---

    [Fact]
    public async Task GetAllAsyncReturnsSuccessWithPagedData()
    {
        // Arrange
        List<Category> items = [new() { Id = Guid.NewGuid(), Name = "Cat 1", Slug = "cat-1" }];
        PagedResult<Category> pagedResult = new(items, 1, 1, 100, 1);

        _repositoryMock.Setup(x => x.GetAllAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        ApiResponse<PagedResult<CategoryDto>> response = await _service.GetAllAsync("testuser", 1, 100, CancellationToken.None);

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
        ApiResponse<PagedResult<CategoryDto>> response = await _service.GetAllAsync("testuser", 1, 100, CancellationToken.None);

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
        Category category = new() { Id = id, Name = "Laptops", Slug = "laptops" };

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        ApiResponse<CategoryDto> response = await _service.GetAsync("testuser", id, CancellationToken.None);

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
            .ReturnsAsync((Category?)null);

        // Act
        ApiResponse<CategoryDto> response = await _service.GetAsync("testuser", Guid.NewGuid(), CancellationToken.None);

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
        ApiResponse<CategoryDto> response = await _service.GetAsync("testuser", Guid.NewGuid(), CancellationToken.None);

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
        Category existingCategory = new() { Id = id, Name = "Old Name", Slug = "old-slug" };
        UpdateCategoryDto dto = new("New Name", "new-slug", null);

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _repositoryMock.Setup(x => x.UpdateAsync(existingCategory, version, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        ApiResponse<CategoryDto> response = await _service.UpdateAsync("testuser", id, version, dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal("New Name", response.Data.Name); // Verifies the mutation happened
    }

    [Fact]
    public async Task UpdateAsyncReturnsNotFoundWhenMissing()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        ApiResponse<CategoryDto> response = await _service.UpdateAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), new UpdateCategoryDto("A", "a", null), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsyncReturnsConflictOnConcurrencyException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Category existingCategory = new() { Id = id };

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _repositoryMock.Setup(x => x.UpdateAsync(existingCategory, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Conflict"));

        // Act
        ApiResponse<CategoryDto> response = await _service.UpdateAsync("testuser", id, Guid.NewGuid(), new UpdateCategoryDto("A", "a", null), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Conflict, response.Code);
    }

    [Fact]
    public async Task UpdateAsyncReturnsFailureOnDbUpdateException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Category existingCategory = new() { Id = id };

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCategory);

        _repositoryMock.Setup(x => x.UpdateAsync(existingCategory, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Duplicate slug"));

        // Act
        ApiResponse<CategoryDto> response = await _service.UpdateAsync("testuser", id, Guid.NewGuid(), new UpdateCategoryDto("A", "a", null), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("database constraint was violated", response.Error, StringComparison.Ordinal);
    }
}