namespace ECommerce.Catalog.Test.RepositoryTests;

public class ProductsRepositoryTests : IDisposable
{
    private readonly MainDbContext _dbContext;
    private readonly Mock<ILogger> _loggerMock;
    private readonly ProductsRepository _productsRepository;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public ProductsRepositoryTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new MainDbContext(options);
        _loggerMock = new Mock<ILogger>();
        _productsRepository = new ProductsRepository(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            _dbContext.Dispose();
        }

        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }

        isDisposed = true;
    }

    [Fact]
    public async Task TestAddAsyncSuccessfullyInsertsProduct()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        _dbContext.Categories.Add(new Category { Id = categoryId, Name = "Test Category", Slug = "test-category" });
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        Product product = new() { Title = "Gaming Laptop", Slug = "gaming-laptop", CategoryId = categoryId };

        // Act
        Product created = await _productsRepository.AddAsync(product, CancellationToken.None);

        // Assert
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Gaming Laptop", created.Title);
        Assert.Equal(1, _dbContext.Products.Count());
    }

    [Fact]
    public async Task TestDeleteAsyncSuccessfullyRemovesProduct()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();

        _dbContext.Categories.Add(new Category { Id = categoryId, Name = "Test Category", Slug = "test-category" });
        Product product = new() { Id = id, Title = "Laptop", Slug = "laptop", CategoryId = categoryId, Version = version };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act
        Product? deleted = await _productsRepository.DeleteAsync(id, version, CancellationToken.None);

        // Assert
        Assert.NotNull(deleted);
        Assert.Equal(id, deleted.Id);
        Assert.Equal(0, _dbContext.Products.Count());
    }

    [Fact]
    public async Task TestDeleteAsyncReturnsNullIfNotFound()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        Product? deleted = await _productsRepository.DeleteAsync(nonExistentId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(deleted);
        Assert.Equal(0, _dbContext.Products.Count());
    }

    [Fact]
    public async Task TestUpdateAsyncForcesModifiedStateAndSaves()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();

        _dbContext.Categories.Add(new Category { Id = categoryId, Name = "Test Category", Slug = "test-category" });
        Product product = new() { Id = id, Title = "Old Title", Slug = "old-slug", CategoryId = categoryId, Version = version };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Simulate fetching the product
        Product trackedProduct = (await _productsRepository.GetAsync(id, CancellationToken.None))!;
        trackedProduct.Title = "New Title";

        // Act - No assignment, just await
        await _productsRepository.UpdateAsync(trackedProduct, version, CancellationToken.None);

        // Assert - Assert against the trackedProduct reference
        Assert.Equal("New Title", trackedProduct.Title);

        Product? dbRecord = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, CancellationToken.None);
        Assert.NotNull(dbRecord);
        Assert.Equal("New Title", dbRecord.Title);

        // Verify the logger was called indicating the update execution
        _loggerMock.Verify(
            x => x.Debug(It.Is<string>(s => s.Contains("Executing UPDATE")), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task TestGetAsyncReturnsProductWithDeepRelations()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        Guid variantId = Guid.NewGuid();
        Guid attributeId = Guid.NewGuid();

        _dbContext.Categories.Add(new Category { Id = categoryId, Name = "Test Category", Slug = "test-category" });
        _dbContext.VariantAttributes.Add(new VariantAttributeModel { Id = attributeId, Name = "Color", Value = "Obsidian Black" });

        Product product = new() { Id = id, Title = "Laptop", Slug = "laptop", CategoryId = categoryId };

        // Wire up the deep relations
        ProductVariant variant = new() { Id = variantId, ProductId = id, Sku = "LAP-123", BasePrice = 1999.99m };
        variant.VariantAttributes.Add(new ProductVariantAttributeModel { VariantId = variantId, AttributeId = attributeId });
        product.Variants.Add(variant);

        product.Media.Add(new ProductMedia { Id = Guid.NewGuid(), ProductId = id, Url = "https://cdn.example.com/laptop.png" });

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Critical: Clear the tracker so EF Core is forced to execute the Includes from the DB!
        _dbContext.ChangeTracker.Clear();

        // Act
        Product? result = await _productsRepository.GetAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);

        // Check Category
        Assert.NotNull(result.Category);
        Assert.Equal("Test Category", result.Category.Name);

        // Check Media
        Assert.Single(result.Media);
        Assert.Equal("https://cdn.example.com/laptop.png", result.Media.First().Url);

        // Check Variants & Attributes
        Assert.Single(result.Variants);
        Assert.Equal("LAP-123", result.Variants.First().Sku);

        Assert.Single(result.Variants.First().VariantAttributes);
        Assert.NotNull(result.Variants.First().VariantAttributes.First().Attribute);
        Assert.Equal("Obsidian Black", result.Variants.First().VariantAttributes.First().Attribute.Value);
    }

    [Fact]
    public async Task TestGetAsyncReturnsNullIfNotFound()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        Product? result = await _productsRepository.GetAsync(nonExistentId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task TestGetAllAsyncReturnsPaginatedResults()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();
        _dbContext.Categories.Add(new Category { Id = categoryId, Name = "Test", Slug = "test" });

        for (int i = 0; i < 15; i++)
        {
            _dbContext.Products.Add(new Product { Id = Guid.NewGuid(), Title = $"Product {i}", Slug = $"product-{i}", CategoryId = categoryId });
        }
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Act: Get page 2, 10 items per page
        PagedResult<Product> result = await _productsRepository.GetAllAsync(pageNumber: 2, itemsPerPage: 10, CancellationToken.None);

        // Assert
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.ItemsPerPage);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(5, result.Items.Count);
        Assert.Empty(_dbContext.ChangeTracker.Entries());
    }
}