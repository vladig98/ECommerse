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
                Gtin: productVariant.Gtin,
                Media: productVariant.Media?.Select(x => x.ToDto()).ToList() ?? [],
                Attributes: productVariant.VariantAttributes?.Select(x => x.Attribute.ToDto()).ToList() ?? []
            );
        }

        public ProductVariantEventDto ToEventData()
        {
            return new ProductVariantEventDto
            (
                Id: productVariant.Id,
                Sku: productVariant.Sku,
                BasePrice: productVariant.BasePrice,
                Gtin: productVariant.Gtin,
                Media: productVariant.Media?.Select(x => x.ToEventData()).ToList() ?? [],
                Attributes: productVariant.VariantAttributes?.Select(x => x.Attribute.ToEventData()).ToList() ?? []
            );
        }
    }
}
