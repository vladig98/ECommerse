namespace ECommerse.Catalog.Dtos.Products;

public record class UpdateProductDto
(
    string Title,
    string Sku,
    string? Description,
    decimal Price,
    int Quantity,
    string CategoryId,
    bool IsActive
);
