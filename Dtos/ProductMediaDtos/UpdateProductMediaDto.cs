namespace ECommerce.Catalog.Dtos.ProductMediaDtos;

public record class UpdateProductMediaDto
(
    string Url,
    string? AltText,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);
