namespace ECommerce.Catalog.Extensions;

public static class ProductVariantExtensions
{
    extension(ProductVariant productVariant)
    {
        public void Update(UpdateProductVariantDto updateProductVariantDto)
        {
            Dictionary<Guid, UpdateProductMediaDto> mediaMap = updateProductVariantDto.Media.ToDictionary(x => x.Id, x => x);

            productVariant.BasePrice = updateProductVariantDto.BasePrice;
            productVariant.Gtin = updateProductVariantDto.Gtin;
            productVariant.Sku = updateProductVariantDto.Sku;
            productVariant.UpdatedAt = DateTime.UtcNow;
            productVariant.VariantAttributes = [.. updateProductVariantDto.Attributes.Select(x => new ProductVariantAttribute() { AttributeId = x })];
            productVariant.Version = Guid.NewGuid();

            foreach (ProductMedia media in productVariant.Media)
            {
                if (!mediaMap.TryGetValue(media.Id, out UpdateProductMediaDto? updateProductMediaDto))
                {
                    continue;
                }

                media.Update(updateProductMediaDto);
            }
        }

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
                Attributes: productVariant.VariantAttributes?
                    .Select(x => x.Attribute?.ToDto())
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .ToList() ?? []
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
                Attributes: productVariant.VariantAttributes?
                    .Select(x => x.Attribute?.ToEventData())
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .ToList() ?? []
            );
        }
    }
}
