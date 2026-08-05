namespace ECommerce.Catalog.Extensions;

internal static class CreateVariantAttributeDtoExtensions
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
