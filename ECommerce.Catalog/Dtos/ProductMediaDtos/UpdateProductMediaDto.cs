namespace ECommerce.Catalog.Dtos.ProductMediaDtos;

public record class UpdateProductMediaDto
(
    Guid Id,
    string Url,
    string? AltText,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);
