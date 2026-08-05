namespace ECommerce.Catalog.Extensions;

internal static class VariantAttributeExtensions
{
    extension(VariantAttributeModel attribute)
    {
        public VariantAttributeDto ToDto()
        {
            return new VariantAttributeDto(
                Id: attribute.Id,
                CreatedAt: attribute.CreatedAt,
                UpdatedAt: attribute.UpdatedAt,
                Version: attribute.Version,
                Name: attribute.Name,
                Value: attribute.Value
            );
        }

        public void Update(UpdateVariantAttributeDto updateVariantAttributeDto)
        {
            attribute.Name = updateVariantAttributeDto.Name;
            attribute.Value = updateVariantAttributeDto.Value;
        }

        public VariantAttributeEventDto ToEventData()
        {
            return new VariantAttributeEventDto
            (
                Id: attribute.Id,
                Name: attribute.Name,
                Value: attribute.Value
            );
        }
    }
}
