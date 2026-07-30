namespace ECommerce.Catalog.Dtos.VariantAttributes;

public record class UpdateVariantAttributeDto
(
    Guid Id,
    Guid Version,
    string Name,
    string Value
);
