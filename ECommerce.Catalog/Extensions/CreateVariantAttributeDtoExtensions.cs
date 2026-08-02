namespace ECommerce.Catalog.Extensions;

public static class CreateVariantAttributeDtoExtensions
{
    extension(CreateVariantAttributeDto createVariantAttributeDto)
    {
        public VariantAttribute ToModel()
        {
            return new VariantAttribute()
            {
                Name = createVariantAttributeDto.Name,
                Value = createVariantAttributeDto.Value
            };
        }

        public ProductVariantAttribute ToModelMap()
        {
            return new ProductVariantAttribute()
            {
                Attribute = createVariantAttributeDto.ToModel()
            };
        }
    }
}
