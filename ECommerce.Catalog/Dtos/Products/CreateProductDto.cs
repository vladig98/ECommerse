namespace ECommerce.Catalog.Dtos.Products;

public record class CreateProductDto
(
    string Title,
    string Slug,
    string? Description,
    string? Brand,
    bool IsActive,
    Guid CategoryId
);
