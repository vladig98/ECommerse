namespace ECommerce.Catalog.Dtos.ProductMediaDtos;

public record class UpdateProductMediaDto
(
    Guid Id,
    Guid Version,
    string Url,
    string? AltText,
    MediaType Type,
    int DisplayOrder,
    bool IsPrimary
);
