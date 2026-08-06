namespace ECommerce.Catalog.Models;

public class ProductMedia : BaseModel
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public MediaType Type { get; set; } = MediaType.Image;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
