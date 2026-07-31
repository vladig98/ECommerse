namespace ECommerce.Catalog.Test.RepositoryTests;

public class ProductMediaRepositoryTests : IDisposable
{
    private readonly MainDbContext dbContext;
    private readonly ProductMediaRepository productMediaRepository;
    private readonly ILogger logger;

    public ProductMediaRepositoryTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        dbContext = new MainDbContext(options);
        logger = new Mock<ILogger>().Object;
        productMediaRepository = new ProductMediaRepository(dbContext, logger);
    }

    public void Dispose()
    {
        dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Test_Create()
    {
        string url = "/images/products/laptops/red-laptop.png";
        string altText = "red laptop";
        MediaType type = MediaType.Image;
        int displayOrder = 0;
        bool isPrimary = true;

        CreateProductMediaDto dto = new(url, altText, type, displayOrder, isPrimary);

        // Act
        ApiResponse<ProductMedia> response = productMediaRepository.Create("TestUser", dto);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductMedia.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<ProductMedia> trackedEntity = dbContext.ChangeTracker.Entries<ProductMedia>().Single();
        Assert.Equal(EntityState.Added, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        ProductMedia productMedia = response.Data;

        Assert.Equal(url, productMedia.Url);
        Assert.Equal(altText, productMedia.AltText);
        Assert.Equal(type, productMedia.Type);
        Assert.Equal(displayOrder, productMedia.DisplayOrder);
        Assert.Equal(isPrimary, productMedia.IsPrimary);

        Assert.Equal(Guid.Empty, productMedia.ProductId);
        Assert.Null(productMedia.Product);

        Assert.Null(productMedia.ProductVariantId);
        Assert.Null(productMedia.ProductVariant);
    }

    [Fact]
    public async Task Test_Delete()
    {
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();

        ProductMedia productMedia = new()
        {
            Url = "/images/products/laptops/red-laptop.png",
            AltText = "red laptop",
            DisplayOrder = 0,
            IsPrimary = true,
            Type = MediaType.Image,
            Version = version,
            Id = id
        };

        dbContext.ProductMedia.Add(productMedia);
        dbContext.SaveChanges();

        // Act
        ApiResponse<ProductMedia> response = await productMediaRepository.DeleteAsync("TestUser", id, version, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST - Fixed DB Set
        int actualSavedRecords = dbContext.ProductMedia.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<ProductMedia> trackedEntity = dbContext.ChangeTracker.Entries<ProductMedia>().Single();
        Assert.Equal(EntityState.Deleted, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        ProductMedia deletedMedia = response.Data;

        Assert.Equal(productMedia.Id, deletedMedia.Id);
        Assert.Equal(productMedia.Url, deletedMedia.Url);
        Assert.Equal(productMedia.AltText, deletedMedia.AltText);
        Assert.Equal(productMedia.Type, deletedMedia.Type);
        Assert.Equal(productMedia.DisplayOrder, deletedMedia.DisplayOrder);
        Assert.Equal(productMedia.IsPrimary, deletedMedia.IsPrimary);
    }

    [Fact]
    public async Task Test_Update()
    {
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();

        ProductMedia productMedia = new()
        {
            Url = "/images/products/laptops/red-laptop.png",
            AltText = "red laptop",
            DisplayOrder = 0,
            IsPrimary = true,
            Type = MediaType.Image,
            Version = version,
            Id = id
        };

        dbContext.ProductMedia.Add(productMedia);
        dbContext.SaveChanges();

        // The URL is changing to prove the update works
        UpdateProductMediaDto dto = new(id, version, "https://hacker.com/you_got_hacked", "red laptop", MediaType.Image, 0, true);

        // Act
        ApiResponse<ProductMedia> response = await productMediaRepository.UpdateAsync("TestUser", id, version, dto, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST - Fixed DB Set
        int actualSavedRecords = dbContext.ProductMedia.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST - Fixed generic type
        EntityEntry<ProductMedia> trackedEntity = dbContext.ChangeTracker.Entries<ProductMedia>().Single();
        Assert.Equal(EntityState.Modified, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        ProductMedia updatedMedia = response.Data;

        Assert.Equal(productMedia.Id, updatedMedia.Id);
        // Assert against the DTO to prove the new values actually mapped
        Assert.Equal(dto.Url, updatedMedia.Url);
        Assert.Equal(dto.AltText, updatedMedia.AltText);
        Assert.Equal(dto.Type, updatedMedia.Type);
        Assert.Equal(dto.DisplayOrder, updatedMedia.DisplayOrder);
        Assert.Equal(dto.IsPrimary, updatedMedia.IsPrimary);
    }

    [Fact]
    public async Task Test_Get()
    {
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();

        // Fixed: We need to setup a ProductMedia, not a Category
        ProductMedia productMedia = new()
        {
            Url = "/images/products/laptops/red-laptop.png",
            AltText = "red laptop",
            DisplayOrder = 0,
            IsPrimary = true,
            Type = MediaType.Image,
            Version = version,
            Id = id
        };

        dbContext.ProductMedia.Add(productMedia);
        dbContext.SaveChanges();

        // Act - Fixed Response Type
        ApiResponse<ProductMedia> response = await productMediaRepository.GetAsync("TestUser", id, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST - Fixed DB Set
        int actualSavedRecords = dbContext.ProductMedia.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST - Fixed generic type
        EntityEntry<ProductMedia> trackedEntity = dbContext.ChangeTracker.Entries<ProductMedia>().Single();
        Assert.Equal(EntityState.Unchanged, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST - Fixed Type and Assertions
        ProductMedia retrievedMedia = response.Data;

        Assert.Equal(productMedia.Id, retrievedMedia.Id);
        Assert.Equal(productMedia.Url, retrievedMedia.Url);
        Assert.Equal(productMedia.AltText, retrievedMedia.AltText);

        Assert.Equal(Guid.Empty, retrievedMedia.ProductId);
        Assert.Null(retrievedMedia.Product);

        Assert.Null(retrievedMedia.ProductVariantId);
        Assert.Null(retrievedMedia.ProductVariant);
    }

    [Fact]
    public async Task Test_Get_All()
    {
        const int numberOfTests = 10;

        List<Guid> ids = [];
        for (int i = 0; i < numberOfTests; i++)
        {
            Guid id = Guid.NewGuid();
            // Fixed: Setting up ProductMedia instead of Category
            ProductMedia productMedia = new()
            {
                Url = $"/images/products/item-{i}.png",
                AltText = $"item {i}",
                DisplayOrder = i,
                IsPrimary = i == 0,
                Type = MediaType.Image,
                Version = Guid.NewGuid(),
                Id = id
            };

            ids.Add(id);
            dbContext.ProductMedia.Add(productMedia);
        }

        dbContext.SaveChanges();

        // Act - Fixed Response Type
        ApiResponse<List<ProductMedia>> response = await productMediaRepository.GetAllAsync("TestUser", CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST - Fixed DB Set
        int actualSavedRecords = dbContext.ProductMedia.Count();
        Assert.Equal(numberOfTests, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST - Fixed Generic Type
        List<EntityEntry<ProductMedia>> trackedEntities = [.. dbContext.ChangeTracker.Entries<ProductMedia>()];
        foreach (EntityEntry<ProductMedia> trackedEntity in trackedEntities)
        {
            Assert.Equal(EntityState.Unchanged, trackedEntity.State);
        }

        // Assert 4: THE RELATIONSHIP TEST - Fixed iterations
        List<ProductMedia> mediaItems = response.Data;
        foreach (ProductMedia media in mediaItems)
        {
            Assert.Contains(media.Id, ids);

            Assert.Equal(Guid.Empty, media.ProductId);
            Assert.Null(media.Product);

            Assert.Null(media.ProductVariantId);
            Assert.Null(media.ProductVariant);
        }
    }

    [Fact]
    public async Task Test_Delete_NotFound()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid nonExistentId = Guid.NewGuid();

        // Act
        ApiResponse<ProductMedia> response = await productMediaRepository.DeleteAsync("TestUser", nonExistentId, version, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the media item does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductMedia.Count();
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

        UpdateProductMediaDto dto = new(nonExistentId, version, "https://hacker.com/image.png", "missing alt", MediaType.Image, 0, true);

        // Act
        ApiResponse<ProductMedia> response = await productMediaRepository.UpdateAsync("TestUser", nonExistentId, version, dto, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the media item does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductMedia.Count();
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
        ApiResponse<ProductMedia> response = await productMediaRepository.GetAsync("TestUser", nonExistentId, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the media item does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.ProductMedia.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }
}