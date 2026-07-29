namespace ECommerce.Catalog.Dtos.ProductMediaDtos;

public record class CreateProductMediaDto
(
    string Url,
    string? AltText,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);