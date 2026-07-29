namespace ECommerce.Catalog.Dtos.ProductMediaDtos;

public readonly record struct ProductMediaDto
(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid Version,
    string Url,
    string? AltText,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);
