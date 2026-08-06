namespace ECommerce.Catalog.Events;

public record class CategoryEventDto
(
    Guid Id,
    string Name,
    string Slug
);