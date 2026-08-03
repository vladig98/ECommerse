namespace ECommerce.Catalog.Test.ServicesTests;

public class VariantAttributeServiceTests
{
    private readonly Mock<IVariantAttributeRepository> mockRepository;
    private readonly Mock<ILogger> mockLogger;
    private readonly VariantAttributeService variantAttributeService;

    public VariantAttributeServiceTests()
    {
        mockRepository = new Mock<IVariantAttributeRepository>();
        mockLogger = new Mock<ILogger>();
        variantAttributeService = new VariantAttributeService(mockRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        mockRepository.Setup(x => x.AddAsync(It.IsAny<VariantAttribute>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException());

        CreateVariantAttributeDto dto = new("Color", "Space Gray");

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.CreateAsync("TestUser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task UpdateAsync_Success_ReturnsMappedDto()
    {
        // Arrange
        VariantAttribute fakeAttribute = new() { Id = Guid.NewGuid(), Name = "Color", Value = "Red" };
        mockRepository.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeAttribute);

        UpdateVariantAttributeDto dto = new("Color", "Blue");

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.UpdateAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Equal("Blue", response.Data!.Value); // Proves domain mutated
        mockRepository.Verify(x => x.UpdateAsync(fakeAttribute, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Success_ReturnsDeletedDto()
    {
        // Arrange
        VariantAttribute fakeAttribute = new() { Id = Guid.NewGuid(), Name = "Color", Value = "Red" };
        mockRepository.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeAttribute);

        // Act
        ApiResponse<VariantAttributeDto> response = await variantAttributeService.DeleteAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Equal("Red", response.Data!.Value);
    }
}