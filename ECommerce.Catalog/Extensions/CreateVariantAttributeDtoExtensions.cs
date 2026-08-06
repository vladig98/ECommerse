namespace ECommerce.Catalog.Extensions;

public static class CreateVariantAttributeDtoExtensions
{
    extension(CreateVariantAttributeDto createVariantAttributeDto)
    {
        public VariantAttributeModel ToModel()
        {
            return new VariantAttributeModel()
            {
                Name = createVariantAttributeDto.Name,
                Value = createVariantAttributeDto.Value
            };
        }
    }
}
