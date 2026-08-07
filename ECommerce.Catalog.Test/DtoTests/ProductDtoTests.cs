namespace ECommerce.Catalog.Test.DtoTests;

public class ProductDtoTests
{
    #region Extension Tests: ToDto

    [Fact]
    public void ToDtoMapsAllPropertiesAndNestedCollectionsCorrectly()
    {
        // Arrange
        Category category = new()
        {
            Id = Guid.NewGuid(),
            Name = "Laptops",
            Slug = "laptops"
        };

        ProductMedia media = new()
        {
            Id = Guid.NewGuid(),
            Url = "https://cdn.example.com/laptop.jpg",
            Type = MediaType.Image,
            IsPrimary = true
        };

        ProductVariant variant = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid(),
            Sku = "LAP-123",
            BasePrice = 1299.99m,
            Gtin = "00011122233344",
            Media = [],
            VariantAttributes = []
        };

        Product product = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid(),
            Title = "Gaming Laptop",
            Slug = "gaming-laptop",
            Description = "High performance laptop",
            Brand = "TechBrand",
            IsActive = true,
            Category = category,
            Media = [media],
            Variants = [variant]
        };

        // Act
        ProductDto dto = product.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(product.Id, dto.Id);
        Assert.Equal(product.Title, dto.Title);
        Assert.Equal(product.Slug, dto.Slug);
        Assert.Equal(product.Description, dto.Description);
        Assert.Equal(product.Brand, dto.Brand);
        Assert.True(dto.IsActive);

        // Assert Nested Mappings
        Assert.NotNull(dto.Category);
        Assert.Equal(category.Id, dto.Category.Id);
        Assert.Single(dto.Media);
        Assert.Equal(media.Id, dto.Media[0].Id);
        Assert.Single(dto.Variants);
        Assert.Equal(variant.Id, dto.Variants[0].Id);
        Assert.Equal(variant.Sku, dto.Variants[0].Sku);
    }

    [Fact]
    public void ToDtoHandlesNullNavigationsAndCollectionsGracefully()
    {
        // Arrange
        Product product = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid(),
            Title = "Generic Product",
            Slug = "generic-product",
            Description = null,
            Brand = null,
            IsActive = false,
            Category = null!,
            Media = null!,
            Variants = null!
        };

        // Act
        ProductDto dto = product.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.Category);
        Assert.Null(dto.Description);
        Assert.Null(dto.Brand);
        Assert.NotNull(dto.Media);
        Assert.Empty(dto.Media);
        Assert.NotNull(dto.Variants);
        Assert.Empty(dto.Variants);
    }

    #endregion

    #region Extension Tests: ToEventData & ToEventDataUpdate

    [Fact]
    public void ToEventDataMapsPopulatedProductToProductCreatedEvent()
    {
        // Arrange
        Category category = new() { Id = Guid.NewGuid(), Name = "Hardware", Slug = "hardware" };
        ProductMedia media = new() { Id = Guid.NewGuid(), Url = "https://cdn.example.com/hw.jpg", IsPrimary = true };
        ProductVariant variant = new() { Id = Guid.NewGuid(), Sku = "HW-1", BasePrice = 49.99m };

        Product product = new()
        {
            Id = Guid.NewGuid(),
            Title = "Motherboard",
            Slug = "motherboard",
            Description = "ATX Board",
            Brand = "GigaTech",
            IsActive = true,
            Category = category,
            Media = [media],
            Variants = [variant]
        };

        // Act
        ProductCreated eventData = product.ToEventData();

        // Assert
        Assert.NotNull(eventData);
        Assert.Equal(product.Id, eventData.Id);
        Assert.Equal(product.Title, eventData.Title);
        Assert.Equal(category.Id, eventData.Category!.Id);
        Assert.Single(eventData.Media);
        Assert.Single(eventData.Variants);
        Assert.Equal("HW-1", eventData.Variants[0].Sku);
    }

    [Fact]
    public void ToEventDataUpdateMapsPopulatedProductToProductUpdatedEvent()
    {
        // Arrange
        Category category = new() { Id = Guid.NewGuid(), Name = "Monitors", Slug = "monitors" };
        Product product = new()
        {
            Id = Guid.NewGuid(),
            Title = "4K Monitor",
            Slug = "4k-monitor",
            Category = category,
            Media = [],
            Variants = []
        };

        // Act
        ProductUpdated eventData = product.ToEventDataUpdate();

        // Assert
        Assert.NotNull(eventData);
        Assert.Equal(product.Id, eventData.Id);
        Assert.Equal(product.Title, eventData.Title);
        Assert.Equal(category.Id, eventData.Category!.Id);
    }

    #endregion

    #region DTO Serialization Tests

    [Fact]
    public void ProductDtoSerializesAndDeserializesCorrectly()
    {
        // Arrange
        ProductDto original = new(
            Id: Guid.NewGuid(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Version: Guid.NewGuid(),
            Title: "Mechanical Keyboard",
            Slug: "mechanical-keyboard",
            Description: "RGB Keyboard",
            Brand: "KeyBrand",
            IsActive: true,
            Category: null,
            Media: [],
            Variants: [],
            LdSchema: null!
        );

        // Act
        string json = JsonSerializer.Serialize(original);
        ProductDto? deserialized = JsonSerializer.Deserialize<ProductDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Title, deserialized.Title);
        Assert.Equal(original.Slug, deserialized.Slug);
    }

    [Fact]
    public void ToEventDataMapsMediaCollectionAndPropertiesCorrectly()
    {
        // Arrange
        ProductMedia primaryMedia = new()
        {
            Id = Guid.NewGuid(),
            Url = "https://cdn.example.com/primary.jpg",
            Type = MediaType.Image,
            IsPrimary = true
        };

        ProductMedia secondaryMedia = new()
        {
            Id = Guid.NewGuid(),
            Url = "https://cdn.example.com/secondary.mp4",
            Type = MediaType.Video,
            IsPrimary = false
        };

        Product product = new()
        {
            Id = Guid.NewGuid(),
            Title = "UltraWide Monitor",
            Slug = "ultrawide-monitor",
            Media = [primaryMedia, secondaryMedia],
            Variants = []
        };

        // Act
        ProductCreated eventData = product.ToEventData();

        // Assert
        Assert.NotNull(eventData);
        Assert.NotNull(eventData.Media);
        Assert.Equal(2, eventData.Media.Count);

        // Assert Primary Media
        ProductMediaEventDto primaryDto = Assert.Single(eventData.Media, m => m.IsPrimary);
        Assert.Equal(primaryMedia.Id, primaryDto.Id);
        Assert.Equal(primaryMedia.Url, primaryDto.Url);
        Assert.Equal(MediaType.Image, primaryDto.Type);

        // Assert Secondary Media
        ProductMediaEventDto secondaryDto = Assert.Single(eventData.Media, m => !m.IsPrimary);
        Assert.Equal(secondaryMedia.Id, secondaryDto.Id);
        Assert.Equal(secondaryMedia.Url, secondaryDto.Url);
        Assert.Equal(MediaType.Video, secondaryDto.Type);
    }

    [Fact]
    public void ToEventDataHandlesNullMediaCollectionWithoutThrowing()
    {
        // Arrange
        Product product = new()
        {
            Id = Guid.NewGuid(),
            Title = "Product Without Media",
            Slug = "no-media-product",
            Media = null!,
            Variants = null!
        };

        // Act
        ProductCreated eventData = product.ToEventData();

        // Assert
        Assert.NotNull(eventData);
        Assert.NotNull(eventData.Media);
        Assert.Empty(eventData.Media);
        Assert.NotNull(eventData.Variants);
        Assert.Empty(eventData.Variants);
    }

    #endregion
}