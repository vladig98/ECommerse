namespace ECommerce.Catalog.Extensions;

public static class ProductVariantExtensions
{
    extension(ProductVariant productVariant)
    {
        public ProductVariantDto ToDto()
        {
            return new ProductVariantDto(
                Id: productVariant.Id,
                CreatedAt: productVariant.CreatedAt,
                UpdatedAt: productVariant.UpdatedAt,
                Version: productVariant.Version,
                Sku: productVariant.Sku,
                BasePrice: productVariant.BasePrice,
                Gtin: productVariant.Gtin
            );
        }
    }
}
