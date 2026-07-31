namespace ECommerce.Catalog.Models;

public class ProductVariant : BaseModel
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Sku { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string? Gtin { get; set; }
    public StockStatus StockStatus { get; set; }

    public ICollection<ProductVariantAttribute> VariantAttributes { get; set; } = [];
    public ICollection<ProductMedia> Media { get; set; } = [];
}
