namespace ECommerce.Catalog.Events;

internal record class ProductVariantEventDto
(
    Guid Id,
    string Sku,
    decimal BasePrice,
    string? Gtin,
    List<ProductMediaEventDto> Media,
    List<VariantAttributeEventDto> Attributes
);