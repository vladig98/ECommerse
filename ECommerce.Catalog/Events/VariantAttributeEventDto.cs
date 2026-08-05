namespace ECommerce.Catalog.Events;

internal record class VariantAttributeEventDto
(
    Guid Id,
    string Name,
    string Value
);
