namespace ECommerce.Catalog.Dtos.Schema;

public record class BrandJsonLdDto
(
    [property: JsonPropertyName("@type")] string Type = "Brand",
    [property: JsonPropertyName("name")] string? Name = null
);
