namespace ECommerce.Catalog.Dtos.Products;

internal record class ProductDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Title,
    string Slug,
    string? Description,
    string? Brand,
    bool IsActive,
    CategoryDto? Category,
    List<ProductMediaDto> Media,
    List<ProductVariantDto> Variants
);
