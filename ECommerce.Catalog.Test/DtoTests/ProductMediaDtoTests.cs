namespace ECommerce.Catalog.Test.DtoTests;

public class ProductMediaDtoTests
{
    #region Extension Tests: ToDto

    [Fact]
    public void ToDto_Maps_All_Properties_Correctly()
    {
        // Arrange
        ProductMedia media = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid(),
            Url = "https://cdn.example.com/images/product1.jpg",
            AltText = "Front view of product",
            Type = MediaType.Image,
            DisplayOrder = 1,
            IsPrimary = true
        };

        // Act
        ProductMediaDto dto = media.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(media.Id, dto.Id);
        Assert.Equal(media.CreatedAt, dto.CreatedAt);
        Assert.Equal(media.UpdatedAt, dto.UpdatedAt);
        Assert.Equal(media.Version, dto.Version);
        Assert.Equal(media.Url, dto.Url);
        Assert.Equal(media.AltText, dto.AltText);
        Assert.Equal(media.Type, dto.Type);
        Assert.Equal(media.DisplayOrder, dto.DisplayOrder);
        Assert.Equal(media.IsPrimary, dto.IsPrimary);
    }

    [Fact]
    public void ToDto_Handles_Null_AltText_Correctly()
    {
        // Arrange
        ProductMedia media = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid(),
            Url = "https://cdn.example.com/videos/demo.mp4",
            AltText = null,
            Type = MediaType.Video,
            DisplayOrder = 2,
            IsPrimary = false
        };

        // Act
        ProductMediaDto dto = media.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.AltText);
        Assert.False(dto.IsPrimary);
    }

    #endregion

    #region Extension Tests: ToEventData

    [Fact]
    public void ToEventData_Maps_All_Event_Properties_Correctly()
    {
        // Arrange
        ProductMedia media = new()
        {
            Id = Guid.NewGuid(),
            Url = "https://cdn.example.com/images/thumb.jpg",
            AltText = "Thumbnail",
            Type = MediaType.Image,
            DisplayOrder = 0,
            IsPrimary = false
        };

        // Act
        ProductMediaEventDto eventDto = media.ToEventData();

        // Assert
        Assert.NotNull(eventDto);
        Assert.Equal(media.Id, eventDto.Id);
        Assert.Equal(media.Url, eventDto.Url);
        Assert.Equal(media.AltText, eventDto.AltText);
        Assert.Equal(media.Type, eventDto.Type);
        Assert.Equal(media.DisplayOrder, eventDto.DisplayOrder);
        Assert.Equal(media.IsPrimary, eventDto.IsPrimary);
    }

    #endregion

    #region DTO Serialization & Record Integrity Tests

    [Fact]
    public void ProductMediaDto_Serializes_And_Deserializes_Correctly()
    {
        // Arrange
        ProductMediaDto original = new(
            Id: Guid.NewGuid(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Version: Guid.NewGuid(),
            Url: "https://cdn.example.com/test.png",
            AltText: "Test Image",
            Type: MediaType.Image,
            DisplayOrder: 3,
            IsPrimary: true
        );

        // Act
        string json = JsonSerializer.Serialize(original);
        ProductMediaDto? deserialized = JsonSerializer.Deserialize<ProductMediaDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Url, deserialized.Url);
        Assert.Equal(original.AltText, deserialized.AltText);
        Assert.Equal(original.Type, deserialized.Type);
        Assert.Equal(original.DisplayOrder, deserialized.DisplayOrder);
        Assert.Equal(original.IsPrimary, deserialized.IsPrimary);
    }

    [Fact]
    public void CreateProductMediaDto_Record_Instantiates_Correctly()
    {
        // Arrange & Act
        CreateProductMediaDto dto = new(
            Url: "https://cdn.example.com/new.jpg",
            AltText: "New Media",
            Type: MediaType.Image,
            DisplayOrder: 1,
            IsPrimary: true
        );

        // Assert
        Assert.Equal("https://cdn.example.com/new.jpg", dto.Url);
        Assert.True(dto.IsPrimary);
    }

    [Fact]
    public void UpdateProductMediaDto_Record_Instantiates_Correctly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();

        // Act
        UpdateProductMediaDto dto = new(
            Id: id,
            Version: version,
            Url: "https://cdn.example.com/updated.jpg",
            AltText: "Updated Alt",
            Type: MediaType.Image,
            DisplayOrder: 5,
            IsPrimary: false
        );

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(version, dto.Version);
        Assert.Equal(5, dto.DisplayOrder);
    }

    #endregion
}