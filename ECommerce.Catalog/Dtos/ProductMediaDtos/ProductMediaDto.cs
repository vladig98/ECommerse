namespace ECommerce.Catalog.Dtos.ProductMediaDtos;

internal record class ProductMediaDto
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
