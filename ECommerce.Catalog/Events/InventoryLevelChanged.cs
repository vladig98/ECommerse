namespace ECommerce.Catalog.Events;

public record class InventoryLevelChanged
(
    Guid VariantId,
    string Sku,
    StockStatus Status
);
