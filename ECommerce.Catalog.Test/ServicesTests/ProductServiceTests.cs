namespace ECommerce.Catalog.Test.ServicesTests;

public class ProductServiceTests : IDisposable
{
    private readonly MainDbContext dbContext;
    private readonly Mock<IProductsRepository> mockProductRepo;
    private readonly Mock<IProductMediaRepository> mockMediaRepo;
    private readonly Mock<IProductVariantRepository> mockVariantRepo;
    private readonly Mock<IVariantAttributeRepository> mockAttributeRepo;
    private readonly Mock<ICategoryRepository> mockCategoryRepo;
    private readonly TestHybridCache testCache;
    private readonly Mock<ILogger> mockLogger;
    private readonly ProductService productService;

    public ProductServiceTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        dbContext = new MainDbContext(options);

        mockProductRepo = new Mock<IProductsRepository>();
        mockMediaRepo = new Mock<IProductMediaRepository>();
        mockVariantRepo = new Mock<IProductVariantRepository>();
        mockAttributeRepo = new Mock<IVariantAttributeRepository>();
        mockCategoryRepo = new Mock<ICategoryRepository>();
        testCache = new TestHybridCache();
        mockLogger = new Mock<ILogger>();

        productService = new ProductService(
            mockProductRepo.Object,
            mockMediaRepo.Object,
            mockVariantRepo.Object,
            mockAttributeRepo.Object,
            mockCategoryRepo.Object,
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
    public async Task CreateAsync_ProductRepoFails_ReturnsFailure_AndNoEvents()
    {
        // Arrange
        mockProductRepo
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<CreateProductDto>()))
            .Returns(ApiResponse<Product>.Failure("Failed to create product"));

        CreateProductDto dto = new("Laptop", "laptop", "Desc", "Brand", false, Guid.NewGuid(), [], []);

        // Act
        ApiResponse<ProductDto> response = await productService.CreateAsync("TestUser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.Generic, response.Code);
        Assert.Null(response.Data);

        Assert.Empty(testCache.SetKeys);
        Assert.Empty(dbContext.EventMessages); // Proves outbox event aborted
    }

    [Fact]
    public async Task CreateAsync_CategoryMissing_ReturnsFailure()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        Product fakeProduct = new() { Id = Guid.NewGuid(), Title = "Laptop", CategoryId = categoryId };

        mockProductRepo
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<CreateProductDto>()))
            .Returns(ApiResponse<Product>.Success(fakeProduct));

        mockCategoryRepo
            .Setup(x => x.GetAsync(It.IsAny<string>(), categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.NotFound("Category missing"));

        CreateProductDto dto = new("Laptop", "laptop", "Desc", "Brand", false, categoryId, [], []);

        // Act
        ApiResponse<ProductDto> response = await productService.CreateAsync("TestUser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Empty(dbContext.EventMessages);
    }

    [Fact]
    public async Task CreateAsync_Success_GeneratesEvent_AndSetsCache()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();

        Product fakeProduct = new() { Id = productId, Title = "Laptop", CategoryId = categoryId, Variants = [], Media = [] };
        Category fakeCategory = new() { Id = categoryId, Name = "Electronics" };

        mockProductRepo
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<CreateProductDto>()))
            .Returns(ApiResponse<Product>.Success(fakeProduct));

        mockCategoryRepo
            .Setup(x => x.GetAsync(It.IsAny<string>(), categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.Success(fakeCategory));

        CreateProductDto dto = new("Laptop", "laptop", "Desc", "Brand", false, categoryId, [], []);

        // Act
        ApiResponse<ProductDto> response = await productService.CreateAsync("TestUser", dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);

        // 1. Verify Cache
        Assert.Contains(CacheKeys.AllProductsKey, testCache.RemovedKeys);
        Assert.Contains(string.Format(CacheKeys.ProductKey, productId), testCache.SetKeys);

        // 2. Verify Event Generation
        Assert.Equal(1, dbContext.EventMessages.Count());
        EventMessage eventMessage = dbContext.EventMessages.Single();
        Assert.Equal(nameof(ProductCreated), eventMessage.EventType);
        Assert.Equal(productId.ToString(), eventMessage.Key);
    }

    // ==========================================
    // DELETE ASYNC PATHS
    // ==========================================

    [Fact]
    public async Task DeleteAsync_RepoFails_ReturnsFailure()
    {
        // Arrange
        mockProductRepo
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Product>.NotFound("Missing"));

        // Act
        ApiResponse<ProductDto> response = await productService.DeleteAsync("TestUser", Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.NotFound, response.Code);
        Assert.Empty(dbContext.EventMessages);
        Assert.Empty(testCache.RemovedKeys);
    }

    [Fact]
    public async Task DeleteAsync_Success_GeneratesEvent_AndRemovesCache()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        Product fakeProduct = new() { Id = productId, Title = "Laptop" };

        mockProductRepo
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), productId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Product>.Success(fakeProduct));

        // Act
        ApiResponse<ProductDto> response = await productService.DeleteAsync("TestUser", productId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);

        // 1. Verify Cache
        Assert.Contains(CacheKeys.AllProductsKey, testCache.RemovedKeys);
        Assert.Contains(string.Format(CacheKeys.ProductKey, productId), testCache.RemovedKeys);

        // 2. Verify Event Generation
        Assert.Equal(1, dbContext.EventMessages.Count());
        EventMessage eventMessage = dbContext.EventMessages.Single();
        Assert.Equal(nameof(ProductDeleted), eventMessage.EventType);
        Assert.Equal(productId.ToString(), eventMessage.Key);
    }

    // ==========================================
    // UPDATE ASYNC PATHS
    // ==========================================

    [Fact]
    public async Task UpdateAsync_Success_WithoutPriceChange_GeneratesOneEvent()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        Guid variantId = Guid.NewGuid();

        // Setup existing product with a base price of 100
        ProductVariant variant = new() { Id = variantId, BasePrice = 100m, Sku = "SKU-1", VariantAttributes = [], Media = [] };
        Product fakeProduct = new() { Id = productId, Title = "Laptop", Variants = [variant], Media = [] };
        Category fakeCategory = new() { Id = categoryId, Name = "Electronics" };

        mockProductRepo
            .Setup(x => x.UpdateAsync(It.IsAny<string>(), productId, It.IsAny<Guid>(), It.IsAny<UpdateProductDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Product>.Success(fakeProduct));

        mockCategoryRepo
            .Setup(x => x.GetAsync(It.IsAny<string>(), categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.Success(fakeCategory));

        // Note: The mocked variant update returns the same base price (100)
        mockVariantRepo
            .Setup(x => x.UpdateAsync(It.IsAny<string>(), variantId, It.IsAny<Guid>(), It.IsAny<UpdateProductVariantDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<ProductVariant>.Success(variant));

        UpdateProductVariantDto variantDto = new(variantId, Guid.NewGuid(), "SKU-1", 100m, null, [], []);
        UpdateProductDto dto = new("Laptop", "laptop", "Desc", "Brand", true, categoryId, [], [variantDto]);

        // Act
        ApiResponse<ProductDto> response = await productService.UpdateAsync("TestUser", productId, Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);

        // Verify ONLY the ProductUpdated event was generated (no price change event)
        Assert.Equal(1, dbContext.EventMessages.Count());
        Assert.Equal(nameof(ProductUpdated), dbContext.EventMessages.Single().EventType);
    }

    [Fact]
    public async Task UpdateAsync_Success_WithPriceChange_GeneratesTwoEvents()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        Guid variantId = Guid.NewGuid();

        // 1. The product repository returns the product with the ORIGINAL price (100)
        ProductVariant variant = new() { Id = variantId, BasePrice = 100m, Sku = "SKU-1", VariantAttributes = [], Media = [] };
        Product fakeProduct = new() { Id = productId, Title = "Laptop", Variants = [variant], Media = [] };
        Category fakeCategory = new() { Id = categoryId, Name = "Electronics" };

        mockProductRepo
            .Setup(x => x.UpdateAsync(It.IsAny<string>(), productId, It.IsAny<Guid>(), It.IsAny<UpdateProductDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Product>.Success(fakeProduct));

        mockCategoryRepo
            .Setup(x => x.GetAsync(It.IsAny<string>(), categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Category>.Success(fakeCategory));

        // 2. We simulate the repository mutating the tracked entity to the NEW price (150)
        ProductVariant updatedVariant = new() { Id = variantId, BasePrice = 150m, Sku = "SKU-1", VariantAttributes = [], Media = [] };

        mockVariantRepo
            .Setup(x => x.UpdateAsync(It.IsAny<string>(), variantId, It.IsAny<Guid>(), It.IsAny<UpdateProductVariantDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string u, Guid id, Guid v, UpdateProductVariantDto d, CancellationToken t) =>
            {
                // Mutate the reference tracked in the fakeProduct to trigger the logic check
                variant.BasePrice = 150m;
                return ApiResponse<ProductVariant>.Success(updatedVariant);
            });

        UpdateProductVariantDto variantDto = new(variantId, Guid.NewGuid(), "SKU-1", 150m, null, [], []);
        UpdateProductDto dto = new("Laptop", "laptop", "Desc", "Brand", true, categoryId, [], [variantDto]);

        // Act
        ApiResponse<ProductDto> response = await productService.UpdateAsync("TestUser", productId, Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);

        // Verify BOTH events were generated
        List<EventMessage> events = [.. dbContext.EventMessages];
        Assert.Equal(2, events.Count);

        Assert.Contains(events, e => e.EventType == nameof(ProductUpdated));
        Assert.Contains(events, e => e.EventType == nameof(ProductPriceChanged));

        // Verify cache was busted and set
        Assert.Contains(CacheKeys.AllProductsKey, testCache.RemovedKeys);
        Assert.Contains(string.Format(CacheKeys.ProductKey, productId), testCache.SetKeys);
    }

    // ==========================================
    // GET & GETALL ASYNC PATHS
    // ==========================================

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        List<Product> fakeProducts = [new Product() { Id = Guid.NewGuid(), Title = "Laptop", Category = new Category() }];

        mockProductRepo
            .Setup(x => x.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<List<Product>>.Success(fakeProducts));

        // Act
        ApiResponse<List<ProductDto>> response = await productService.GetAllAsync("TestUser", CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.Single(response.Data!);
        Assert.Equal("Laptop", response.Data![0].Title);
    }

    [Fact]
    public async Task GetAsync_ReturnsMappedDto()
    {
        // Arrange
        Guid productId = Guid.NewGuid();
        Product fakeProduct = new() { Id = productId, Title = "Laptop", Category = new Category() };

        mockProductRepo
            .Setup(x => x.GetAsync(It.IsAny<string>(), productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<Product>.Success(fakeProduct));

        // Act
        ApiResponse<ProductDto> response = await productService.GetAsync("TestUser", productId, CancellationToken.None);

        // Assert
        Assert.Equal(ErrorCodes.None, response.Code);
        Assert.NotNull(response.Data);
        Assert.Equal("Laptop", response.Data.Title);
    }
}