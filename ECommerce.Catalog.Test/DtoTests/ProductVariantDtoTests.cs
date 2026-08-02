namespace ECommerce.Catalog.Test.DtoTests;

public class ProductVariantDtoTests
{
    #region Extension Tests: ToDto

    [Fact]
    public void ToDto_Maps_All_Properties_And_Nested_Collections_Correctly()
    {
        // Arrange
        ProductMedia media = new()
        {
            Id = Guid.NewGuid(),
            Url = "https://cdn.example.com/variant-black.jpg",
            Type = MediaType.Image,
            IsPrimary = true
        };

        VariantAttribute attribute = new()
        {
            Id = Guid.NewGuid(),
            Name = "Color",
            Value = "Black"
        };

        ProductVariantAttribute joinEntity = new()
        {
            AttributeId = attribute.Id,
            Attribute = attribute
        };

        ProductVariant variant = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid(),
            Sku = "SKU-BLK-123",
            BasePrice = 99.99m,
            Gtin = "01234567890123",
            StockStatus = StockStatus.InStock,
            Media = [media],
            VariantAttributes = [joinEntity]
        };

        // Act
        ProductVariantDto dto = variant.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(variant.Id, dto.Id);
        Assert.Equal(variant.Sku, dto.Sku);
        Assert.Equal(variant.BasePrice, dto.BasePrice);
        Assert.Equal(variant.Gtin, dto.Gtin);

        // Media mapping
        Assert.Single(dto.Media);
        Assert.Equal(media.Id, dto.Media[0].Id);

        // Attributes mapping via join entity
        Assert.Single(dto.Attributes);
        Assert.Equal(attribute.Id, dto.Attributes[0].Id);
        Assert.Equal(attribute.Name, dto.Attributes[0].Name);
        Assert.Equal(attribute.Value, dto.Attributes[0].Value);
    }

    [Fact]
    public void ToDto_Handles_Null_Collections_And_Gtin_Gracefully()
    {
        // Arrange
        ProductVariant variant = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid(),
            Sku = "SKU-SIMPLE",
            BasePrice = 19.99m,
            Gtin = null,
            StockStatus = StockStatus.OutOfStock,
            Media = null!,
            VariantAttributes = null!
        };

        // Act
        ProductVariantDto dto = variant.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.Gtin);
        Assert.NotNull(dto.Media);
        Assert.Empty(dto.Media);
        Assert.NotNull(dto.Attributes);
        Assert.Empty(dto.Attributes);
    }

    #endregion

    #region Extension Tests: ToEventData

    [Fact]
    public void ToEventData_Maps_All_Event_Properties_Correctly()
    {
        // Arrange
        ProductMedia media = new() { Id = Guid.NewGuid(), Url = "https://cdn.example.com/thumb.jpg" };
        VariantAttribute attribute = new() { Id = Guid.NewGuid(), Name = "Size", Value = "XL" };
        ProductVariantAttribute joinEntity = new() { Attribute = attribute };

        ProductVariant variant = new()
        {
            Id = Guid.NewGuid(),
            Sku = "SKU-XL",
            BasePrice = 49.50m,
            Gtin = "98765432109876",
            StockStatus = StockStatus.InStock,
            Media = [media],
            VariantAttributes = [joinEntity]
        };

        // Act
        ProductVariantEventDto eventDto = variant.ToEventData();

        // Assert
        Assert.NotNull(eventDto);
        Assert.Equal(variant.Id, eventDto.Id);
        Assert.Equal(variant.Sku, eventDto.Sku);
        Assert.Equal(variant.BasePrice, eventDto.BasePrice);
        Assert.Equal(variant.Gtin, eventDto.Gtin);
        Assert.Single(eventDto.Media);
        Assert.Single(eventDto.Attributes);
        Assert.Equal("Size", eventDto.Attributes[0].Name);
    }

    #endregion

    #region DTO Serialization & Record Tests

    [Fact]
    public void ProductVariantDto_Serializes_And_Deserializes_Correctly()
    {
        // Arrange
        ProductVariantDto original = new(
            Id: Guid.NewGuid(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Version: Guid.NewGuid(),
            Sku: "SKU-TEST",
            BasePrice: 150.00m,
            Gtin: "11122233344455",
            Media: [],
            Attributes: []
        );

        // Act
        string json = JsonSerializer.Serialize(original);
        ProductVariantDto? deserialized = JsonSerializer.Deserialize<ProductVariantDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Sku, deserialized.Sku);
        Assert.Equal(original.BasePrice, deserialized.BasePrice);
    }

    #endregion
}