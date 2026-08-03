namespace ECommerce.Catalog.Test.ServicesTests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> mockRepository;
    private readonly Mock<ILogger> mockLogger;
    private readonly CategoryService categoryService;

    public CategoryServiceTests()
    {
        mockRepository = new Mock<ICategoryRepository>();
        mockLogger = new Mock<ILogger>();
        categoryService = new CategoryService(mockRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        mockRepository.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException());

        // Act
        ApiResponse<CategoryDto> response = await categoryService.CreateAsync("TestUser", new CreateCategoryDto("Laptops", "laptops", null), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code); // Service maps DbUpdateException to Generic Failure
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task CreateAsync_Success_ReturnsDto()
    {
        // Arrange
        Category fakeCategory = new() { Id = Guid.NewGuid(), Name = "Laptops", Slug = "laptops" };
        mockRepository.Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeCategory);

        // Act
        ApiResponse<CategoryDto> response = await categoryService.CreateAsync("TestUser", new CreateCategoryDto("Laptops", "laptops", null), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedDtos()
    {
        // Arrange
        PagedResult<Category> pagedResult = new([new Category { Name = "Test" }], 1, 1, 100, 1);
        mockRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        ApiResponse<PagedResult<CategoryDto>> response = await categoryService.GetAllAsync("TestUser", 1, 100, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Single(response.Data!.Items);
    }
}