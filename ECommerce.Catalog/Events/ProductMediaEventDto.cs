namespace ECommerce.Catalog.Events;

internal record class ProductMediaEventDto
(
    Guid Id,
    string Url,
    string? AltText,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);
