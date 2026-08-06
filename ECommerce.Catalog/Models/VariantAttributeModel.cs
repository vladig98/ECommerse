namespace ECommerce.Catalog.Models;

public class VariantAttributeModel : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public ICollection<ProductVariantAttributeModel> ProductVariants { get; set; } = [];
}
