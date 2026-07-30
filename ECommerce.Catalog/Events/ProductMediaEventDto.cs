namespace ECommerce.Catalog.Events;

public record class ProductMediaEventDto
(
    Guid Id,
    string Url,
    string? AltText,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);
