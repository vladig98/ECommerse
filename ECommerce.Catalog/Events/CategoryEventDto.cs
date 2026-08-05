namespace ECommerce.Catalog.Events;

internal record class CategoryEventDto
(
    Guid Id,
    string Name,
    string Slug
);