namespace ECommerce.Catalog.Dtos.ProductVariants;

public record class UpdateProductVariantDto
(
    Guid Id,
    Guid Version,
    string Sku,
    decimal BasePrice,
    string? Gtin,
    List<UpdateProductMediaDto> Media,
    List<Guid> Attributes
);
