namespace ECommerce.Catalog.Models;

internal class ProductVariantAttributeModel
{
    public Guid VariantId { get; set; }
    public ProductVariant Variant { get; set; } = null!;

    public Guid AttributeId { get; set; }
    public VariantAttributeModel Attribute { get; set; } = null!;
}
