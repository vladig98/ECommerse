namespace ECommerce.Catalog.Dtos.Schema;

public record class OfferJsonLdDto
(
    [property: JsonPropertyName("@type")] string Type = "Offer",
    [property: JsonPropertyName("sku")] string? Sku = null,
    [property: JsonPropertyName("priceCurrency")] string PriceCurrency = "USD",
    [property: JsonPropertyName("price")] decimal? Price = null,
    [property: JsonPropertyName("availability")] string? Availability = null
);
