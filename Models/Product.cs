namespace ECommerce.Catalog.Models;

public class Product : BaseModel
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ProductMedia> Media { get; set; } = [];
    public ICollection<ProductVariant> Variants { get; set; } = [];
}
