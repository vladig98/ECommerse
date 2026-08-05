namespace ECommerce.Catalog.Dtos.VariantAttributes;

internal record class VariantAttributeDto
(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Name,
    string Value
);
