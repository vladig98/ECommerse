namespace ECommerce.Catalog.Dtos.Products;

internal record class UpdateProductDto
(
    string Title,
    string Slug,
    string? Description,
    string? Brand,
    bool IsActive,
    Guid CategoryId,
    List<UpdateProductMediaDto> ProductMedia,
    List<UpdateProductVariantDto> ProductVariants
);
