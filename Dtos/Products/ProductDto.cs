namespace ECommerce.Catalog.Dtos.Products;

public readonly record struct ProductDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Title,
    string Slug,
    string? Description,
    string? Brand,
    bool IsActive,
    Guid CategoryId
);
