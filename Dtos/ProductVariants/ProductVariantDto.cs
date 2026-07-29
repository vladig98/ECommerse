namespace ECommerce.Catalog.Dtos.ProductVariants;

public readonly record struct ProductVariantDto
(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Sku,
    decimal BasePrice,
    string? Gtin
);
