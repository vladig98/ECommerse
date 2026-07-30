namespace ECommerce.Catalog.Events;

public record class VariantAttributeEventDto
(
    Guid Id,
    string Name,
    string Value
);
