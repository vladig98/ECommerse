namespace ECommerce.Catalog.Dtos.ProductMediaDtos;

internal record class CreateProductMediaDto
(
    string Url,
    string? AltText,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);