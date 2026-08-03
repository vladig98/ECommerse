namespace ECommerce.Catalog.Test.ServicesTests;

public class ProductServiceTests
{
    private readonly Mock<IProductsRepository> mockRepository;
    private readonly Mock<ILogger> mockLogger;
    private readonly ProductService productService;

    public ProductServiceTests()
    {
        mockRepository = new Mock<IProductsRepository>();
        mockLogger = new Mock<ILogger>();
        productService = new ProductService(mockRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        mockRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB Error"));

        CreateProductDto dto = new("Laptop", "laptop", "Desc", "Brand", false, Guid.NewGuid(), [], []);

        // Act
        ApiResponse<ProductDto> response = await productService.CreateAsync("TestUser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNotFoundResponse()
    {
        // Arrange
        mockRepository.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        UpdateProductDto dto = new("Laptop", "laptop", "Desc", "Brand", true, Guid.NewGuid(), [], []);

        // Act
        ApiResponse<ProductDto> response = await productService.UpdateAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task UpdateAsync_Success_ReturnsDto()
    {
        // Arrange
        Product fakeProduct = new() { Id = Guid.NewGuid(), Title = "Old Title" };
        mockRepository.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeProduct);

        UpdateProductDto dto = new("New Title", "laptop", "Desc", "Brand", true, Guid.NewGuid(), [], []);

        // Act
        ApiResponse<ProductDto> response = await productService.UpdateAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Equal("New Title", response.Data!.Title); // Proves domain mutation worked
        mockRepository.Verify(x => x.UpdateAsync(fakeProduct, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}