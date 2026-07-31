namespace ECommerce.Catalog.Test.RepositoryTests;

public class ProductsRepositoryTests : IDisposable
{
    private readonly MainDbContext dbContext;
    private readonly ProductsRepository productsRepository;
    private readonly ILogger logger;

    public ProductsRepositoryTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        dbContext = new MainDbContext(options);
        logger = new Mock<ILogger>().Object;
        productsRepository = new ProductsRepository(dbContext, logger);
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
        string title = "Gaming Laptop";
        string slug = "gaming-laptop";
        string description = "A high end gaming laptop";
        string brand = "TechBrand";
        bool isActive = false;
        Guid categoryId = Guid.NewGuid();

        CreateProductDto dto = new(title, slug, description, brand, isActive, categoryId, [], []);

        // Act
        ApiResponse<Product> response = productsRepository.Create("TestUser", dto);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Products.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<Product> trackedEntity = dbContext.ChangeTracker.Entries<Product>().Single();
        Assert.Equal(EntityState.Added, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        Product product = response.Data;

        Assert.Equal(title, product.Title);
        Assert.Equal(slug, product.Slug);
        Assert.Equal(description, product.Description);
        Assert.Equal(brand, product.Brand);
        Assert.True(product.IsActive);

        Assert.Equal(Guid.Empty, product.CategoryId);
        Assert.Null(product.Category);
        Assert.Empty(product.Media);
        Assert.Empty(product.Variants);
    }

    [Fact]
    public async Task Test_Delete()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();

        Category dummyCategory = new()
        {
            Id = categoryId,
            Name = "Test Category",
            Slug = "test-category"
        };

        dbContext.Categories.Add(dummyCategory);

        Product product = new()
        {
            Id = id,
            CategoryId = categoryId,
            Title = "Gaming Laptop",
            Slug = "gaming-laptop",
            Description = "A high end gaming laptop",
            Brand = "TechBrand",
            IsActive = true,
            Version = version
        };

        dbContext.Products.Add(product);
        dbContext.SaveChanges();

        // Act
        ApiResponse<Product> response = await productsRepository.DeleteAsync("TestUser", id, version, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Products.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<Product> trackedEntity = dbContext.ChangeTracker.Entries<Product>().Single();
        Assert.Equal(EntityState.Deleted, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        Product deletedProduct = response.Data;
        Assert.Equal(product.Id, deletedProduct.Id);
        Assert.Equal(product.Title, deletedProduct.Title);
    }

    [Fact]
    public async Task Test_Update()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Guid oldCategoryId = Guid.NewGuid();

        Category dummyCategory = new()
        {
            Id = oldCategoryId,
            Name = "Test Category",
            Slug = "test-category"
        };

        dbContext.Categories.Add(dummyCategory);

        Product product = new()
        {
            Id = id,
            Title = "Old Title",
            Slug = "old-slug",
            Description = "Old desc",
            Brand = "Old Brand",
            IsActive = false,
            CategoryId = oldCategoryId,
            Version = version
        };

        dbContext.Products.Add(product);
        dbContext.SaveChanges();

        Guid newCategoryId = Guid.NewGuid();
        Category newDummyCategory = new()
        {
            Id = newCategoryId,
            Name = "Test Category",
            Slug = "test-category"
        };

        dbContext.Categories.Add(newDummyCategory);

        UpdateProductDto dto = new("New Title", "new-slug", "New desc", "New Brand", true, newCategoryId, [], []);

        // Act
        ApiResponse<Product> response = await productsRepository.UpdateAsync("TestUser", id, version, dto, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Products.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<Product> trackedEntity = dbContext.ChangeTracker.Entries<Product>().Single();
        Assert.Equal(EntityState.Modified, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        Product updatedProduct = response.Data;

        Assert.Equal(product.Id, updatedProduct.Id);

        // Assert the new values mapped successfully
        Assert.Equal(dto.Title, updatedProduct.Title);
        Assert.Equal(dto.Slug, updatedProduct.Slug);
        Assert.Equal(dto.Description, updatedProduct.Description);
        Assert.Equal(dto.Brand, updatedProduct.Brand);
        Assert.Equal(dto.IsActive, updatedProduct.IsActive);
        Assert.Equal(dto.CategoryId, updatedProduct.CategoryId);
    }

    [Fact]
    public async Task Test_Get()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();

        Category dummyCategory = new()
        {
            Id = categoryId,
            Name = "Test Category",
            Slug = "test-category"
        };

        dbContext.Categories.Add(dummyCategory);

        Product product = new()
        {
            Id = id,
            CategoryId = categoryId,
            Title = "Gaming Laptop",
            Slug = "gaming-laptop",
            Description = "A high end gaming laptop",
            Brand = "TechBrand",
            IsActive = true,
            Version = version
        };

        dbContext.Products.Add(product);
        dbContext.SaveChanges();

        // Act
        ApiResponse<Product> response = await productsRepository.GetAsync("TestUser", id, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Products.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<Product> trackedEntity = dbContext.ChangeTracker.Entries<Product>().Single();
        Assert.Equal(EntityState.Unchanged, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        Product retrievedProduct = response.Data;

        Assert.Equal(product.Id, retrievedProduct.Id);
        Assert.Equal(product.Title, retrievedProduct.Title);
        Assert.Equal(product.Slug, retrievedProduct.Slug);

        // Proves collections initialized properly even if empty
        Assert.NotNull(retrievedProduct.Media);
        Assert.NotNull(retrievedProduct.Variants);
    }

    [Fact]
    public async Task Test_Get_All()
    {
        // Arrange
        const int numberOfTests = 10;
        List<Guid> ids = [];

        for (int i = 0; i < numberOfTests; i++)
        {
            Guid id = Guid.NewGuid();
            Product product = new()
            {
                Id = id,
                Title = $"Product {i}",
                Slug = $"product-{i}",
                Description = "Desc",
                Brand = "Brand",
                IsActive = true,
                Version = Guid.NewGuid()
            };

            ids.Add(id);
            dbContext.Products.Add(product);
        }

        dbContext.SaveChanges();

        // Act
        ApiResponse<List<Product>> response = await productsRepository.GetAllAsync("TestUser", CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Products.Count();
        Assert.Equal(numberOfTests, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        List<EntityEntry<Product>> trackedEntities = [.. dbContext.ChangeTracker.Entries<Product>()];
        foreach (EntityEntry<Product> trackedEntity in trackedEntities)
        {
            Assert.Equal(EntityState.Unchanged, trackedEntity.State);
        }

        // Assert 4: THE RELATIONSHIP TEST
        List<Product> products = response.Data;
        foreach (Product prod in products)
        {
            Assert.Contains(prod.Id, ids);
        }
    }

    [Fact]
    public async Task Test_Delete_NotFound()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid nonExistentId = Guid.NewGuid();

        // Act
        ApiResponse<Product> response = await productsRepository.DeleteAsync("TestUser", nonExistentId, version, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the product does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Products.Count();
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
        Guid newCategoryId = Guid.NewGuid();

        UpdateProductDto dto = new("New Title", "new-slug", "New desc", "New Brand", true, newCategoryId, [], []);

        // Act
        ApiResponse<Product> response = await productsRepository.UpdateAsync("TestUser", nonExistentId, version, dto, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the product does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Products.Count();
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
        ApiResponse<Product> response = await productsRepository.GetAsync("TestUser", nonExistentId, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the product does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.Products.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }
}