namespace ECommerce.Catalog.Dtos.VariantAttributes;

public readonly record struct VariantAttributeDto
(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Name,
    string Value
);
