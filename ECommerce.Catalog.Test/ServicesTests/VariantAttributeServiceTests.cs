namespace ECommerce.Catalog.Test.ServicesTests;

public class VariantAttributeServiceTests
{
    private readonly Mock<IVariantAttributeRepository> _repositoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly VariantAttributeService _service;

    public VariantAttributeServiceTests()
    {
        _repositoryMock = new Mock<IVariantAttributeRepository>();
        _loggerMock = new Mock<ILogger>();
        _service = new VariantAttributeService(_repositoryMock.Object, _loggerMock.Object);
    }

    // --- CreateAsync Tests ---

    [Fact]
    public async Task CreateAsyncReturnsSuccessWhenValid()
    {
        // Arrange
        CreateVariantAttributeDto dto = new("Color", "Red");
        VariantAttributeModel createdAttribute = new() { Id = Guid.NewGuid(), Name = "Color", Value = "Red" };

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<VariantAttributeModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAttribute);

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal(createdAttribute.Id, response.Data.Id);
        Assert.Null(response.Error);
    }

    [Fact]
    public async Task CreateAsyncReturnsFailureOnDbUpdateException()
    {
        // Arrange
        CreateVariantAttributeDto dto = new("Color", "Red");

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<VariantAttributeModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Constraint violation"));

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.CreateAsync("testuser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Null(response.Data);
        Assert.Contains("database constraint was violated", response.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAsyncReturnsFailureOnGenericException()
    {
        // Arrange
        CreateVariantAttributeDto dto = new("Color", "Red");

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<VariantAttributeModel>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Catastrophic failure"));

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.CreateAsync("testuser", dto, CancellationToken.None);

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
        VariantAttributeModel deletedAttribute = new() { Id = id, Name = "Color", Value = "Red" };

        _repositoryMock.Setup(x => x.DeleteAsync(id, version, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedAttribute);

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.DeleteAsync("testuser", id, version, CancellationToken.None);

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
            .ReturnsAsync((VariantAttributeModel?)null);

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

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
        ApiResponse<VariantAttributeDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

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
        ApiResponse<VariantAttributeDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("currently assigned to one or more product variants", response.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeleteAsyncReturnsFailureOnGenericException()
    {
        // Arrange
        _repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Generic error"));

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.DeleteAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("unexpected error occurred", response.Error, StringComparison.Ordinal);
    }

    // --- GetAllAsync Tests ---

    [Fact]
    public async Task GetAllAsyncReturnsSuccessWithPagedData()
    {
        // Arrange
        List<VariantAttributeModel> items = [new() { Id = Guid.NewGuid(), Name = "Size", Value = "XL" }];
        PagedResult<VariantAttributeModel> pagedResult = new(items, 1, 1, 100, 1);

        _repositoryMock.Setup(x => x.GetAllAsync(1, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        ApiResponse<PagedResult<VariantAttributeDto>> response = await _service.GetAllAsync("testuser", 1, 100, CancellationToken.None);

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
        ApiResponse<PagedResult<VariantAttributeDto>> response = await _service.GetAllAsync("testuser", 1, 100, CancellationToken.None);

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
        VariantAttributeModel attribute = new() { Id = id, Name = "Color", Value = "Blue" };

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attribute);

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.GetAsync("testuser", id, CancellationToken.None);

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
            .ReturnsAsync((VariantAttributeModel?)null);

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.GetAsync("testuser", Guid.NewGuid(), CancellationToken.None);

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
        ApiResponse<VariantAttributeDto> response = await _service.GetAsync("testuser", Guid.NewGuid(), CancellationToken.None);

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
        VariantAttributeModel existingAttribute = new() { Id = id, Name = "Color", Value = "Old Red" };
        UpdateVariantAttributeDto dto = new("Color", "New Red");

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAttribute);

        _repositoryMock.Setup(x => x.UpdateAsync(existingAttribute, version, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.UpdateAsync("testuser", id, version, dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal("New Red", response.Data.Value); // Verifies the mutation happened
    }

    [Fact]
    public async Task UpdateAsyncReturnsNotFoundWhenMissing()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VariantAttributeModel?)null);

        UpdateVariantAttributeDto dto = new("Color", "New Blue");

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.UpdateAsync("testuser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<VariantAttributeModel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsyncReturnsConflictOnConcurrencyException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        VariantAttributeModel existingAttribute = new() { Id = id };
        UpdateVariantAttributeDto dto = new("Color", "New Blue");

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAttribute);

        _repositoryMock.Setup(x => x.UpdateAsync(existingAttribute, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Conflict"));

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.UpdateAsync("testuser", id, Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Conflict, response.Code);
    }

    [Fact]
    public async Task UpdateAsyncReturnsFailureOnDbUpdateException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        VariantAttributeModel existingAttribute = new() { Id = id };
        UpdateVariantAttributeDto dto = new("Color", "New Blue");

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAttribute);

        _repositoryMock.Setup(x => x.UpdateAsync(existingAttribute, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Duplicate unique key"));

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.UpdateAsync("testuser", id, Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("database constraint was violated", response.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateAsyncReturnsFailureOnGenericException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        VariantAttributeModel existingAttribute = new() { Id = id };
        UpdateVariantAttributeDto dto = new("Color", "New Blue");

        _repositoryMock.Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAttribute);

        _repositoryMock.Setup(x => x.UpdateAsync(existingAttribute, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Random failure"));

        // Act
        ApiResponse<VariantAttributeDto> response = await _service.UpdateAsync("testuser", id, Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Contains("unexpected error occurred", response.Error, StringComparison.Ordinal);
    }
}