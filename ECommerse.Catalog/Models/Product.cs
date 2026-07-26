namespace ECommerse.Catalog.Models;

public class Product : BaseModel
{
    public string Title { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;

    public Category Category { get; set; } = null!;
    public string CategoryId { get; set; } = string.Empty;

    public ProductDto ToDto()
    {
        return new ProductDto(
            Id: Id,
            CreatedAt: CreatedAt,
            UpdatedAt: UpdatedAt,
            Version: Version,
            Title: Title,
            Sku: Sku,
            Description: Description,
            Price: Price,
            Quantity: StockQuantity,
            IsActive: IsActive,
            CategoryId: CategoryId
        );
    }
}
