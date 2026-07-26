namespace ECommerse.Catalog.Dtos.Products;

public record class CreateProductDto
(
    string Title,
    string Sku,
    string? Description,
    decimal Price,
    int Quantity,
    string CategoryId
);
