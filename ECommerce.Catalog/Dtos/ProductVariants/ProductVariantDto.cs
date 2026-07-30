namespace ECommerce.Catalog.Dtos.ProductVariants;

public record class ProductVariantDto
(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Sku,
    decimal BasePrice,
    string? Gtin,
    List<ProductMediaDto> Media,
    List<VariantAttributeDto> Attributes
);
