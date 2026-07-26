namespace ECommerce.Catalog.Models;

public class VariantAttribute : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public ICollection<ProductVariantAttribute> ProductVariants { get; set; } = [];
}
