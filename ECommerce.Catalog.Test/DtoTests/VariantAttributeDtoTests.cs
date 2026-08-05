namespace ECommerce.Catalog.Test.DtoTests;

public class VariantAttributeDtoTests
{
    #region Extension Tests: ToDto

    [Fact]
    public void ToDtoMapsAllPropertiesCorrectly()
    {
        // Arrange
        VariantAttributeModel attribute = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = Guid.NewGuid(),
            Name = "Color",
            Value = "Midnight Blue"
        };

        // Act
        VariantAttributeDto dto = attribute.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(attribute.Id, dto.Id);
        Assert.Equal(attribute.CreatedAt, dto.CreatedAt);
        Assert.Equal(attribute.UpdatedAt, dto.UpdatedAt);
        Assert.Equal(attribute.Version, dto.Version);
        Assert.Equal(attribute.Name, dto.Name);
        Assert.Equal(attribute.Value, dto.Value);
    }

    #endregion

    #region Extension Tests: ToEventData

    [Fact]
    public void ToEventDataMapsAllEventPropertiesCorrectly()
    {
        // Arrange
        VariantAttributeModel attribute = new()
        {
            Id = Guid.NewGuid(),
            Name = "Size",
            Value = "XXL"
        };

        // Act
        VariantAttributeEventDto eventDto = attribute.ToEventData();

        // Assert
        Assert.NotNull(eventDto);
        Assert.Equal(attribute.Id, eventDto.Id);
        Assert.Equal(attribute.Name, eventDto.Name);
        Assert.Equal(attribute.Value, eventDto.Value);
    }

    #endregion

    #region DTO Serialization & Record Integrity Tests

    [Fact]
    public void VariantAttributeDtoSerializesAndDeserializesCorrectly()
    {
        // Arrange
        VariantAttributeDto original = new(
            Id: Guid.NewGuid(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            Version: Guid.NewGuid(),
            Name: "Storage",
            Value: "512GB"
        );

        // Act
        string json = JsonSerializer.Serialize(original);
        VariantAttributeDto? deserialized = JsonSerializer.Deserialize<VariantAttributeDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Value, deserialized.Value);
    }

    [Fact]
    public void CreateVariantAttributeDtoRecordInstantiatesCorrectly()
    {
        // Arrange & Act
        CreateVariantAttributeDto dto = new(Name: "RAM", Value: "16GB");

        // Assert
        Assert.Equal("RAM", dto.Name);
        Assert.Equal("16GB", dto.Value);
    }

    [Fact]
    public void UpdateVariantAttributeDtoRecordInstantiatesCorrectly()
    {
        // Act
        UpdateVariantAttributeDto dto = new(
            Name: "Weight",
            Value: "1.5kg"
        );

        // Assert
        Assert.Equal("Weight", dto.Name);
        Assert.Equal("1.5kg", dto.Value);
    }

    #endregion
}