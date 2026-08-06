namespace ECommerce.Catalog.Events;

public record class ProductCreated
(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string? Brand,
    bool IsActive,
    CategoryEventDto? Category,
    List<ProductMediaEventDto> Media,
    List<ProductVariantEventDto> Variants
);