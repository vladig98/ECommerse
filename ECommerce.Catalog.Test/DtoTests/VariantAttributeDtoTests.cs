namespace ECommerce.Catalog.Test.DtoTests;

public class VariantAttributeDtoTests
{
    #region Extension Tests: ToDto

    [Fact]
    public void ToDto_Maps_All_Properties_Correctly()
    {
        // Arrange
        VariantAttribute attribute = new()
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
    public void ToEventData_Maps_All_Event_Properties_Correctly()
    {
        // Arrange
        VariantAttribute attribute = new()
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
    public void VariantAttributeDto_Serializes_And_Deserializes_Correctly()
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
    public void CreateVariantAttributeDto_Record_Instantiates_Correctly()
    {
        // Arrange & Act
        CreateVariantAttributeDto dto = new(Name: "RAM", Value: "16GB");

        // Assert
        Assert.Equal("RAM", dto.Name);
        Assert.Equal("16GB", dto.Value);
    }

    [Fact]
    public void UpdateVariantAttributeDto_Record_Instantiates_Correctly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid version = Guid.NewGuid();

        // Act
        UpdateVariantAttributeDto dto = new(
            Id: id,
            Version: version,
            Name: "Weight",
            Value: "1.5kg"
        );

        // Assert
        Assert.Equal(id, dto.Id);
        Assert.Equal(version, dto.Version);
        Assert.Equal("Weight", dto.Name);
        Assert.Equal("1.5kg", dto.Value);
    }

    #endregion
}