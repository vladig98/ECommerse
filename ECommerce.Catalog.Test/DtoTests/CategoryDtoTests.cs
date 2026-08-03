namespace ECommerce.Catalog.Test.DtoTests;

public class CategoryDtoTests
{
    #region Extension Tests: ToDto

    [Fact]
    public void ToDto_Returns_Null_When_Category_Is_Null()
    {
        // Arrange
        Category? category = null;

        // Act
        CategoryDto? dto = category.ToDto();

        // Assert
        Assert.Null(dto);
    }

    [Fact]
    public void ToDto_Maps_All_Basic_Properties_Correctly()
    {
        // Arrange
        Category category = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid(),
            Name = "Electronics",
            Slug = "electronics",
            ParentCategory = null,
            SubCategories = []
        };

        // Act
        CategoryDto? dto = category.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(category.Id, dto.Id);
        Assert.Equal(category.CreatedAt, dto.CreatedAt);
        Assert.Equal(category.UpdatedAt, dto.UpdatedAt);
        Assert.Equal(category.Version, dto.Version);
        Assert.Equal(category.Name, dto.Name);
        Assert.Equal(category.Slug, dto.Slug);
        Assert.Null(dto.ParentCategory);
        Assert.Empty(dto.SubCategories);
    }

    [Fact]
    public void ToDto_Handles_Null_SubCategories_Collection_On_Entity()
    {
        // Arrange
        Category category = new()
        {
            Id = Guid.NewGuid(),
            Name = "Hardware",
            Slug = "hardware",
            SubCategories = null!
        };

        // Act
        CategoryDto? dto = category.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.SubCategories);
        Assert.Empty(dto.SubCategories);
    }

    [Fact]
    public void ToDto_Recursively_Maps_Parent_And_SubCategories()
    {
        // Arrange
        Category parentCategory = new()
        {
            Id = Guid.NewGuid(),
            Name = "Computers",
            Slug = "computers"
        };

        Category subCategory = new()
        {
            Id = Guid.NewGuid(),
            Name = "Gaming Laptops",
            Slug = "gaming-laptops"
        };

        Category category = new()
        {
            Id = Guid.NewGuid(),
            Name = "Laptops",
            Slug = "laptops",
            ParentCategory = parentCategory,
            SubCategories = [subCategory]
        };

        // Act
        CategoryDto? dto = category.ToDto();

        // Assert
        Assert.NotNull(dto);

        // Verify Parent Mapping
        Assert.NotNull(dto.ParentCategory);
        Assert.Equal(parentCategory.Id, dto.ParentCategory.Id);
        Assert.Equal("Computers", dto.ParentCategory.Name);

        // Verify Child Collection Mapping
        Assert.Single(dto.SubCategories);
        Assert.Equal(subCategory.Id, dto.SubCategories[0]?.Id);
        Assert.Equal("Gaming Laptops", dto.SubCategories[0]?.Name);
    }

    [Fact]
    public void ToDto_Handles_Null_Elements_Inside_SubCategories_List()
    {
        // Arrange
        Category category = new()
        {
            Id = Guid.NewGuid(),
            Name = "Peripherals",
            Slug = "peripherals",
            SubCategories = [null!]
        };

        // Act
        CategoryDto? dto = category.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Single(dto.SubCategories);
        Assert.Null(dto.SubCategories[0]);
    }

    #endregion

    #region Extension Tests: ToEventData

    [Fact]
    public void ToEventData_Maps_Correct_Fields()
    {
        // Arrange
        Category category = new()
        {
            Id = Guid.NewGuid(),
            Name = "Home & Kitchen",
            Slug = "home-kitchen"
        };

        // Act
        CategoryEventDto? eventData = category.ToEventData();

        // Assert
        Assert.NotNull(eventData);
        Assert.Equal(category.Id, eventData.Id);
        Assert.Equal(category.Name, eventData.Name);
        Assert.Equal(category.Slug, eventData.Slug);
    }

    [Fact]
    public void ToEventData_Returns_Null_When_Category_Is_Null()
    {
        // Arrange
        Category? category = null;

        // Act & Assert
        CategoryEventDto? eventData = category.ToEventData();
        Assert.Null(eventData);
    }

    #endregion

    #region DTO Contract & Equality Tests

    [Fact]
    public void CategoryDto_Record_Value_Equality_Works_As_Expected()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        Guid version = Guid.NewGuid();
        List<CategoryDto?> subCategories = [];

        CategoryDto dto1 = new(id, now, now, version, "Phones", "phones", null, subCategories);
        CategoryDto dto2 = new(id, now, now, version, "Phones", "phones", null, subCategories);

        // Act & Assert
        Assert.Equal(dto1, dto2);
    }

    [Fact]
    public void CreateCategoryDto_Record_Instantiates_Correctly()
    {
        // Arrange
        Guid? parentId = Guid.NewGuid();

        // Act
        CreateCategoryDto dto = new("Smartphones", "smartphones", parentId);

        // Assert
        Assert.Equal("Smartphones", dto.Name);
        Assert.Equal("smartphones", dto.Slug);
        Assert.Equal(parentId, dto.ParentCategoryId);
    }

    [Fact]
    public void UpdateCategoryDto_Record_Value_Equality_Works_As_Expected()
    {
        // Arrange
        Guid parentId = Guid.NewGuid();
        UpdateCategoryDto dto1 = new("Audio", "audio", parentId);
        UpdateCategoryDto dto2 = new("Audio", "audio", parentId);

        // Assert
        Assert.Equal(dto1, dto2);
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void CategoryDto_Serializes_And_Deserializes_Correctly()
    {
        // Arrange
        CategoryDto original = new(
            Id: Guid.NewGuid(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Version: Guid.NewGuid(),
            Name: "Monitors",
            Slug: "monitors",
            ParentCategory: null,
            SubCategories: []
        );

        // Act
        string json = JsonSerializer.Serialize(original);
        CategoryDto? deserialized = JsonSerializer.Deserialize<CategoryDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Slug, deserialized.Slug);
    }

    #endregion
}