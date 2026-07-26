namespace ECommerse.Catalog.Dtos.Products;

public readonly record struct ProductDto(
    string Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Title,
    string Sku,
    string? Description,
    decimal Price,
    int Quantity,
    bool IsActive,
    string CategoryId
);
