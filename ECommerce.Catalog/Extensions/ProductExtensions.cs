namespace ECommerce.Catalog.Extensions;

public static class ProductExtensions
{
    extension(Product product)
    {
        public void Update(UpdateProductDto updateProductDto)
        {
            product.Brand = updateProductDto.Brand;
            product.CategoryId = updateProductDto.CategoryId;
            product.Description = updateProductDto.Description;
            product.IsActive = updateProductDto.IsActive;
            product.Slug = updateProductDto.Slug;
            product.Title = updateProductDto.Title;

            UpdateProductMedia(product, updateProductDto);
            UpdateProductVariant(product, updateProductDto);
        }

        public ProductDto ToDto()
        {
            return new ProductDto
            (
                Id: product.Id,
                CreatedAt: product.CreatedAt,
                UpdatedAt: product.UpdatedAt,
                Version: product.Version,
                Title: product.Title,
                Slug: product.Slug,
                Description: product.Description,
                Brand: product.Brand,
                IsActive: product.IsActive,
                Category: product.Category?.ToDto(),
                Media: product.Media?.Select(x => x.ToDto()).ToList() ?? [],
                Variants: product.Variants?.Select(x => x.ToDto()).ToList() ?? []
            );
        }

        public ProductCreated ToEventData()
        {
            return new ProductCreated
            (
                Id: product.Id,
                Title: product.Title,
                Slug: product.Slug,
                Description: product.Description,
                Brand: product.Brand,
                IsActive: product.IsActive,
                Category: product.Category.ToEventData(),
                Media: product.Media?.Select(x => x.ToEventData()).ToList() ?? [],
                Variants: product.Variants?.Select(x => x.ToEventData()).ToList() ?? []
            );
        }

        public ProductUpdated ToEventDataUpdate()
        {
            return new ProductUpdated
            (
                Id: product.Id,
                Title: product.Title,
                Slug: product.Slug,
                Description: product.Description,
                Brand: product.Brand,
                IsActive: product.IsActive,
                Category: product.Category.ToEventData(),
                Media: product.Media?.Select(x => x.ToEventData()).ToList() ?? [],
                Variants: product.Variants?.Select(x => x.ToEventData()).ToList() ?? []
            );
        }

        public ProductDeleted ToEventDataDelete()
        {
            return new ProductDeleted
            (
                Id: product.Id
            );
        }
    }

    private static void UpdateProductVariant(Product product, UpdateProductDto updateProductDto)
    {
        Dictionary<Guid, UpdateProductVariantDto> variantMap = [];
        List<ProductVariant> variantsToInsert = [];

        foreach (UpdateProductVariantDto updateVariantDto in updateProductDto.ProductVariants)
        {
            if (updateVariantDto.Id.Equals(default) || updateVariantDto.Id.Equals(Guid.Empty))
            {
                variantsToInsert.Add(updateVariantDto.ToModel());
                continue;
            }

            variantMap[updateVariantDto.Id] = updateVariantDto;
        }

        List<ProductVariant> variantsToRemove = [];
        foreach (ProductVariant variant in product.Variants)
        {
            if (!variantMap.TryGetValue(variant.Id, out UpdateProductVariantDto? variantDto))
            {
                variantsToRemove.Add(variant);
                continue;
            }

            variant.Update(variantDto);
        }

        foreach (ProductVariant toRemove in variantsToRemove)
        {
            product.Variants.Remove(toRemove);
        }

        foreach (ProductVariant toInsert in variantsToInsert)
        {
            product.Variants.Add(toInsert);
        }
    }

    private static void UpdateProductMedia(Product product, UpdateProductDto updateProductDto)
    {
        Dictionary<Guid, UpdateProductMediaDto> mediaMap = [];
        List<ProductMedia> mediaToInsert = [];

        foreach (UpdateProductMediaDto updateMediaDto in updateProductDto.ProductMedia)
        {
            if (updateMediaDto.Id.Equals(default) || updateMediaDto.Id.Equals(Guid.Empty))
            {
                mediaToInsert.Add(updateMediaDto.ToModel());
                continue;
            }

            mediaMap[updateMediaDto.Id] = updateMediaDto;
        }

        List<ProductMedia> mediaToRemove = [];
        foreach (ProductMedia media in product.Media)
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
            product.Media.Remove(toRemove);
        }

        foreach (ProductMedia toInsert in mediaToInsert)
        {
            product.Media.Add(toInsert);
        }
    }
}
