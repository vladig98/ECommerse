namespace ECommerce.Catalog.Dtos.Schema;

public record class ProductJsonLdDto
(
    [property: JsonPropertyName("@context")] string Context = "https://schema.org/",
    [property: JsonPropertyName("@type")] string Type = "Product",
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("brand")] BrandJsonLdDto? Brand = null,
    [property: JsonPropertyName("offers")] List<OfferJsonLdDto>? Offers = null
);
