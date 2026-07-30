namespace ECommerce.Catalog.Extensions;

public static class VariantAttributeExtensions
{
    extension(VariantAttribute attribute)
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
