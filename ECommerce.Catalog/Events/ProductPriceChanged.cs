namespace ECommerce.Catalog.Events;

public record class ProductPriceChanged
(
    Guid ProductId,
    Guid VariantId,
    string Sku,
    decimal NewPrice
);
