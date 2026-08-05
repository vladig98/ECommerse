namespace ECommerce.Catalog.Events;

internal record class ProductPriceChanged
(
    Guid ProductId,
    Guid VariantId,
    string Sku,
    decimal NewPrice
);
