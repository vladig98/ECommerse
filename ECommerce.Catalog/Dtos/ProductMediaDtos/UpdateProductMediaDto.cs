namespace ECommerce.Catalog.Dtos.ProductMediaDtos;

internal record class UpdateProductMediaDto
(
    Guid Id,
    string Url,
    string? AltText,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);
