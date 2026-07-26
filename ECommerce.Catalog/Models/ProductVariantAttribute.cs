namespace ECommerce.Catalog.Models;

public class ProductVariantAttribute : BaseModel
{
    public Guid VariantId { get; set; }
    public ProductVariant Variant { get; set; } = null!;

    public Guid AttributeId { get; set; }
    public VariantAttribute Attribute { get; set; } = null!;
}
