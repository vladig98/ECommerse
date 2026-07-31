namespace ECommerce.Catalog.Test.RepositoryTests;

public class VariantAttributeRepositoryTests : IDisposable
{
    private readonly MainDbContext dbContext;
    private readonly VariantAttributeRepository variantAttributeRepository;
    private readonly ILogger logger;

    public VariantAttributeRepositoryTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        dbContext = new MainDbContext(options);
        logger = new Mock<ILogger>().Object;
        variantAttributeRepository = new VariantAttributeRepository(dbContext, logger);
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
        string name = "Color";
        string value = "Space Gray";

        CreateVariantAttributeDto dto = new(name, value);

        // Act
        ApiResponse<VariantAttribute> response = variantAttributeRepository.Create("TestUser", dto);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.VariantAttributes.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<VariantAttribute> trackedEntity = dbContext.ChangeTracker.Entries<VariantAttribute>().Single();
        Assert.Equal(EntityState.Added, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        VariantAttribute attribute = response.Data;

        Assert.Equal(name, attribute.Name);
        Assert.Equal(value, attribute.Value);

        Assert.Empty(attribute.ProductVariants);
    }

    [Fact]
    public async Task Test_Delete()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();

        VariantAttribute attribute = new()
        {
            Id = id,
            Name = "Color",
            Value = "Space Gray",
            Version = version
        };

        dbContext.VariantAttributes.Add(attribute);
        dbContext.SaveChanges();

        // Act
        ApiResponse<VariantAttribute> response = await variantAttributeRepository.DeleteAsync("TestUser", id, version, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.VariantAttributes.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<VariantAttribute> trackedEntity = dbContext.ChangeTracker.Entries<VariantAttribute>().Single();
        Assert.Equal(EntityState.Deleted, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        VariantAttribute deletedAttribute = response.Data;
        Assert.Equal(attribute.Id, deletedAttribute.Id);
        Assert.Equal(attribute.Name, deletedAttribute.Name);
        Assert.Equal(attribute.Value, deletedAttribute.Value);
    }

    [Fact]
    public async Task Test_Update()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();

        VariantAttribute attribute = new()
        {
            Id = id,
            Name = "Old Name",
            Value = "Old Value",
            Version = version
        };

        dbContext.VariantAttributes.Add(attribute);
        dbContext.SaveChanges();

        UpdateVariantAttributeDto dto = new(id, version, "New Name", "New Value");

        // Act
        ApiResponse<VariantAttribute> response = await variantAttributeRepository.UpdateAsync("TestUser", id, version, dto, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.VariantAttributes.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<VariantAttribute> trackedEntity = dbContext.ChangeTracker.Entries<VariantAttribute>().Single();
        Assert.Equal(EntityState.Modified, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        VariantAttribute updatedAttribute = response.Data;

        Assert.Equal(attribute.Id, updatedAttribute.Id);

        // Assert the new values mapped successfully
        Assert.Equal(dto.Name, updatedAttribute.Name);
        Assert.Equal(dto.Value, updatedAttribute.Value);
    }

    [Fact]
    public async Task Test_Get()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid id = Guid.NewGuid();

        VariantAttribute attribute = new()
        {
            Id = id,
            Name = "Color",
            Value = "Space Gray",
            Version = version
        };

        dbContext.VariantAttributes.Add(attribute);
        dbContext.SaveChanges();

        // Act
        ApiResponse<VariantAttribute> response = await variantAttributeRepository.GetAsync("TestUser", id, CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.VariantAttributes.Count();
        Assert.Equal(1, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        EntityEntry<VariantAttribute> trackedEntity = dbContext.ChangeTracker.Entries<VariantAttribute>().Single();
        Assert.Equal(EntityState.Unchanged, trackedEntity.State);

        // Assert 4: THE RELATIONSHIP TEST
        VariantAttribute retrievedAttribute = response.Data;

        Assert.Equal(attribute.Id, retrievedAttribute.Id);
        Assert.Equal(attribute.Name, retrievedAttribute.Name);
        Assert.Equal(attribute.Value, retrievedAttribute.Value);

        Assert.NotNull(retrievedAttribute.ProductVariants);
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
            VariantAttribute attribute = new()
            {
                Id = id,
                Name = $"Attribute {i}",
                Value = $"Value {i}",
                Version = Guid.NewGuid()
            };

            ids.Add(id);
            dbContext.VariantAttributes.Add(attribute);
        }

        dbContext.SaveChanges();

        // Act
        ApiResponse<List<VariantAttribute>> response = await variantAttributeRepository.GetAllAsync("TestUser", CancellationToken.None);

        // Assert 1: Basic success
        Assert.True(string.IsNullOrEmpty(response.Error), "Expected no error");
        Assert.NotNull(response.Data);
        Assert.Equal(ErrorCodes.None, response.Code);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.VariantAttributes.Count();
        Assert.Equal(numberOfTests, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        List<EntityEntry<VariantAttribute>> trackedEntities = [.. dbContext.ChangeTracker.Entries<VariantAttribute>()];
        foreach (EntityEntry<VariantAttribute> trackedEntity in trackedEntities)
        {
            Assert.Equal(EntityState.Unchanged, trackedEntity.State);
        }

        // Assert 4: THE RELATIONSHIP TEST
        List<VariantAttribute> attributes = response.Data;
        foreach (VariantAttribute attr in attributes)
        {
            Assert.Contains(attr.Id, ids);
        }
    }

    [Fact]
    public async Task Test_Delete_NotFound()
    {
        // Arrange
        Guid version = Guid.NewGuid();
        Guid nonExistentId = Guid.NewGuid();

        // Act
        ApiResponse<VariantAttribute> response = await variantAttributeRepository.DeleteAsync("TestUser", nonExistentId, version, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the attribute does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.VariantAttributes.Count();
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

        UpdateVariantAttributeDto dto = new(nonExistentId, version, "New Name", "New Value");

        // Act
        ApiResponse<VariantAttribute> response = await variantAttributeRepository.UpdateAsync("TestUser", nonExistentId, version, dto, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the attribute does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.VariantAttributes.Count();
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
        ApiResponse<VariantAttribute> response = await variantAttributeRepository.GetAsync("TestUser", nonExistentId, CancellationToken.None);

        // Assert 1: Expected Failure
        Assert.False(string.IsNullOrEmpty(response.Error), "Expected an error message because the attribute does not exist.");
        Assert.Null(response.Data);
        Assert.Contains("could not be found", response.Error);

        // Assert 2: THE UNIT OF WORK TEST
        int actualSavedRecords = dbContext.VariantAttributes.Count();
        Assert.Equal(0, actualSavedRecords);

        // Assert 3: STATE TRACKING TEST
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }
}