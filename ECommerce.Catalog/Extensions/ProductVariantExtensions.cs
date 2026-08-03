namespace ECommerce.Catalog.Extensions;

public static class ProductVariantExtensions
{
    extension(ProductVariant productVariant)
    {
        public void Update(UpdateProductVariantDto updateProductVariantDto)
        {
            productVariant.BasePrice = updateProductVariantDto.BasePrice;
            productVariant.Gtin = updateProductVariantDto.Gtin;
            productVariant.Sku = updateProductVariantDto.Sku;

            UpdateProductVariantMedia(productVariant, updateProductVariantDto);
            UpdateProductVariantAttributes(productVariant, updateProductVariantDto);
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

        public ProductPriceChanged ToPricheChangeEventData()
        {
            return new ProductPriceChanged
            (
                ProductId: productVariant.ProductId,
                VariantId: productVariant.Id,
                Sku: productVariant.Sku,
                NewPrice: productVariant.BasePrice
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

    private static void UpdateProductVariantMedia(ProductVariant productVariant, UpdateProductVariantDto updateProductVariantDto)
    {
        Dictionary<Guid, UpdateProductMediaDto> mediaMap = [];
        List<ProductMedia> mediaToInsert = [];

        foreach (UpdateProductMediaDto updateMediaDto in updateProductVariantDto.Media)
        {
            if (updateMediaDto.Id.Equals(default) || updateMediaDto.Id.Equals(Guid.Empty))
            {
                mediaToInsert.Add(updateMediaDto.ToModel());
                continue;
            }

            mediaMap[updateMediaDto.Id] = updateMediaDto;
        }

        List<ProductMedia> mediaToRemove = [];
        foreach (ProductMedia media in productVariant.Media)
        {
            if (!mediaMap.TryGetValue(media.Id, out UpdateProductMediaDto? mediaDto))
            {
                mediaToRemove.Add(media);
                continue;
            }

            media.Update(mediaDto);
        }

        foreach (ProductMedia toRemove in mediaToRemove)
        {
            productVariant.Media.Remove(toRemove);
        }

        foreach (ProductMedia toInsert in mediaToInsert)
        {
            productVariant.Media.Add(toInsert);
        }
    }

    private static void UpdateProductVariantAttributes(ProductVariant productVariant, UpdateProductVariantDto updateProductVariantDto)
    {
        HashSet<Guid> incomingIds = [.. updateProductVariantDto.Attributes];
        HashSet<Guid> existingIds = [.. productVariant.VariantAttributes.Select(x => x.AttributeId)];
        
        List<ProductVariantAttribute> toRemove = [];
        foreach (ProductVariantAttribute variantAttribute in productVariant.VariantAttributes)
        {
            if (incomingIds.Contains(variantAttribute.AttributeId))
            {
                continue;
            }

            toRemove.Add(variantAttribute);
        }

        List<Guid> toInsert = [];
        foreach (Guid attributeId in updateProductVariantDto.Attributes)
        {
            if (existingIds.Contains(attributeId))
            {
                continue;
            }

            toInsert.Add(attributeId);
        }

        foreach (ProductVariantAttribute attributeToRemove in toRemove)
        {
            productVariant.VariantAttributes.Remove(attributeToRemove);
        }

        foreach (Guid attributeId in toInsert)
        {
            productVariant.VariantAttributes.Add(new ProductVariantAttribute()
            {
                AttributeId = attributeId,
                VariantId = productVariant.Id
            });
        }
    }
}
