namespace ECommerce.Catalog.Events;

internal record class InventoryLevelChanged
(
    Guid VariantId,
    string Sku,
    StockStatus Status
);
