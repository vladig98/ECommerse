namespace ECommerce.Catalog.Dtos.ProductVariants;

public record class UpdateProductVariantDto
(
    string Sku,
    decimal BasePrice,
    string? Gtin,
    List<UpdateVariantAttributeDto> VariantAttributes,
    List<UpdateProductMediaDto> VariantMedia
);
