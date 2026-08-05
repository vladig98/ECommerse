namespace ECommerce.Catalog.Dtos.ProductVariants;

internal record class UpdateProductVariantDto
(
    Guid Id,
    string Sku,
    decimal BasePrice,
    string? Gtin,
    List<UpdateProductMediaDto> Media,
    List<Guid> Attributes
);
