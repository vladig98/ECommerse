namespace ECommerce.Catalog.Dtos.ProductVariants;

public record class CreateProductVariantDto
(
    string Sku,
    decimal BasePrice,
    string? Gtin,
    List<CreateProductMediaDto> Media,
    List<CreateVariantAttributeDto> Attributes
);
