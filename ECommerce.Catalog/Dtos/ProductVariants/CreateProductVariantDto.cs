namespace ECommerce.Catalog.Dtos.ProductVariants;

internal record class CreateProductVariantDto
(
    string Sku,
    decimal BasePrice,
    string? Gtin,
    List<CreateProductMediaDto> Media,
    List<Guid> Attributes
);
