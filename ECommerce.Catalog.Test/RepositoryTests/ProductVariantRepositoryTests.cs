namespace ECommerce.Catalog.Test.RepositoryTests;

public class ProductVariantRepositoryTests : IDisposable
{
    private readonly MainDbContext dbContext;
    private readonly ProductVariantRepository productVariantRepository;
    private readonly ILogger logger;

    public ProductVariantRepositoryTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        dbContext = new MainDbContext(options);
        logger = new Mock<ILogger>().Object;
        productVariantRepository = new ProductVariantRepository(dbContext, logger);
    }

    public void Dispose()
    {
        dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Test_Create()
    {
        // Arrange
        string sku = "LAPTOP-RED-16GB";
        decimal basePrice = 1499.99m;
        string gtin = "1234567890123";

        CreateProductVariantDto dto = new(sku, basePrice, gtin, [], [], []);

        // Act
        ApiResponse<ProductVariant> response = productVariantRepository.Create("TestUser", dto);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductVariants.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<ProductVariant> trackedEntity = dbContext.ChangeTracker.Entries<ProductVariant>().Single();
        Assert.Equal(EntityState.Added, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        ProductVariant variant = response.Data;

        Assert.Equal(sku, variant.Sku);
        Assert.Equal(basePrice, variant.BasePrice);
        Assert.Equal(gtin, variant.Gtin);

        Assert.Equal(Guid.Empty, variant.ProductId);
        Assert.Null(variant.Product);
        Assert.Empty(variant.VariantAttributes);
        Assert.Empty(variant.Media);
    }

    [Fact]
    public async Task Test_Delete()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Guid productId = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();

        // Seeding the parent chain to perfectly mirror your product test safety
        Category dummyCategory = new() { Id = categoryId, Name = "Cat", Slug = "cat" };
        Product dummyProduct = new() { Id = productId, CategoryId = categoryId, Title = "Prod", Slug = "prod" };

        dbContext.Categories.Add(dummyCategory);
        dbContext.Products.Add(dummyProduct);

        ProductVariant variant = new()
        {
            Id = id,
            ProductId = productId,
            Sku = "LAPTOP-RED-16GB",
            BasePrice = 1499.99m,
            Gtin = "1234567890123",
            Version = version
        };

        dbContext.ProductVariants.Add(variant);
        dbContext.SaveChanges();

        // Act
        ApiResponse<ProductVariant> response = await productVariantRepository.DeleteAsync("TestUser", id, version, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductVariants.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<ProductVariant> trackedEntity = dbContext.ChangeTracker.Entries<ProductVariant>().Single();
        Assert.Equal(EntityState.Deleted, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        ProductVariant deletedVariant = response.Data;
        Assert.Equal(variant.Id, deletedVariant.Id);
        Assert.Equal(variant.Sku, deletedVariant.Sku);
    }

    [Fact]
    public async Task Test_Update()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        Product dummyProduct = new() { Id = productId, Title = "Prod", Slug = "prod" };
        dbContext.Products.Add(dummyProduct);

        ProductVariant variant = new()
        {
            Id = id,
            ProductId = productId,
            Sku = "OLD-SKU",
            BasePrice = 100m,
            Gtin = "OLD-GTIN",
            Version = version
        };

        dbContext.ProductVariants.Add(variant);
        dbContext.SaveChanges();

        UpdateProductVariantDto dto = new(id, version, "NEW-SKU", 150m, "NEW-GTIN", [], []);

        // Act
        ApiResponse<ProductVariant> response = await productVariantRepository.UpdateAsync("TestUser", id, version, dto, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductVariants.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<ProductVariant> trackedEntity = dbContext.ChangeTracker.Entries<ProductVariant>().Single();
        Assert.Equal(EntityState.Modified, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        ProductVariant updatedVariant = response.Data;

        Assert.Equal(variant.Id, updatedVariant.Id);

        // Assert the new values mapped successfully
        Assert.Equal(dto.Sku, updatedVariant.Sku);
        Assert.Equal(dto.BasePrice, updatedVariant.BasePrice);
        Assert.Equal(dto.Gtin, updatedVariant.Gtin);
    }

    [Fact]
    public async Task Test_Get()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Guid productId = Guid.NewGuid();

        Product dummyProduct = new() { Id = productId, Title = "Prod", Slug = "prod" };
        dbContext.Products.Add(dummyProduct);

        ProductVariant variant = new()
        {
            Id = id,
            ProductId = productId,
            Sku = "LAPTOP-RED-16GB",
            BasePrice = 1499.99m,
            Gtin = "1234567890123",
            Version = version
        };

        dbContext.ProductVariants.Add(variant);
        dbContext.SaveChanges();

        // Act
        ApiResponse<ProductVariant> response = await productVariantRepository.GetAsync("TestUser", id, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductVariants.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<ProductVariant> trackedEntity = dbContext.ChangeTracker.Entries<ProductVariant>().Single();
        Assert.Equal(EntityState.Unchanged, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        ProductVariant retrievedVariant = response.Data;

        Assert.Equal(variant.Id, retrievedVariant.Id);
        Assert.Equal(variant.Sku, retrievedVariant.Sku);
        Assert.Equal(variant.BasePrice, retrievedVariant.BasePrice);

        // Proves collections initialized properly even if empty
        Assert.NotNull(retrievedVariant.VariantAttributes);
        Assert.NotNull(retrievedVariant.Media);
    }

    [Fact]
    public async Task Test_Get_All()
    {
        // Arrange
        const int numberOfTests = 10;
        List<Guid> ids = [];

        Guid productId = Guid.NewGuid();
        Product dummyProduct = new() { Id = productId, Title = "Prod", Slug = "prod" };
        dbContext.Products.Add(dummyProduct);

        for (int i = 0; i < numberOfTests; i++)
        {
            Guid id = Guid.NewGuid();
            ProductVariant variant = new()
            {
                Id = id,
                ProductId = productId,
                Sku = $"SKU-{i}",
                BasePrice = 100m + i,
                Version = Guid.NewGuid()
            };

            ids.Add(id);
            dbContext.ProductVariants.Add(variant);
        }

        dbContext.SaveChanges();

        // Act
        ApiResponse<List<ProductVariant>> response = await productVariantRepository.GetAllAsync("TestUser", CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductVariants.Count();
        Assert.Equal(numberOfTests, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        List<EntityEntry<ProductVariant>> trackedEntities = [.. dbContext.ChangeTracker.Entries<ProductVariant>()];
        foreach (EntityEntry<ProductVariant> trackedEntity in trackedEntities)
        {
            Assert.Equal(EntityState.Unchanged, trackedEntity.State);
        }

        // Assert 4: THE RELATIONSHIP TEST
        List<ProductVariant> variants = response.Data;
        foreach (ProductVariant var in variants)
        {
            Assert.Contains(var.Id, ids);
        }
    }

    [Fact]
    public async Task Test_Delete_NotFound()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid nonExistentId = Guid.NewGuid();

        // Act
        ApiResponse<ProductVariant> response = await productVariantRepository.DeleteAsync("TestUser", nonExistentId, version, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the variant does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductVariants.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task Test_Update_NotFound()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid nonExistentId = Guid.NewGuid();

        UpdateProductVariantDto dto = new(nonExistentId, version, "NEW-SKU", 150m, "NEW-GTIN", [], []);

        // Act
        ApiResponse<ProductVariant> response = await productVariantRepository.UpdateAsync("TestUser", nonExistentId, version, dto, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the variant does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductVariants.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    [Fact]
    public async Task Test_Get_NotFound()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        ApiResponse<ProductVariant> response = await productVariantRepository.GetAsync("TestUser", nonExistentId, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the variant does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductVariants.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }
}