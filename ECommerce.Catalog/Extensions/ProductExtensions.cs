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
            product.UpdatedAt = DateTime.UtcNow;
            product.Version = Guid.NewGuid();

            Dictionary<Guid, UpdateProductMediaDto> mediaMap = updateProductDto.ProductMedia.ToDictionary(x => x.Id, x => x);
            List<ProductMedia> itemsToRemove = [.. product.Media.Where(m => !mediaMap.ContainsKey(m.Id))];

            foreach (ProductMedia? item in itemsToRemove)
            {
                product.Media.Remove(item);
            }

            foreach (UpdateProductMediaDto mediaDto in updateProductDto.ProductMedia)
            {
                ProductMedia? existingMedia = product.Media.FirstOrDefault(m => m.Id == mediaDto.Id);
                if (existingMedia is not null)
                {
                    existingMedia.Update(mediaDto);
                }
                else
                {
                    product.Media.Add(mediaDto.ToModel());
                }
            }

            Dictionary<Guid, UpdateProductVariantDto> variantsMap = updateProductDto.ProductVariants.ToDictionary(x => x.Id, x => x);
            List<ProductVariant> variantsToRemove = [.. product.Variants.Where(v => !variantsMap.ContainsKey(v.Id))];

            foreach (ProductVariant variant in variantsToRemove)
            {
                product.Variants.Remove(variant);
            }

            foreach (UpdateProductVariantDto variantDto in updateProductDto.ProductVariants)
            {
                ProductVariant? existingVariant = product.Variants.FirstOrDefault(v => v.Id == variantDto.Id);
                if (existingVariant is not null)
                {
                    existingVariant.Update(variantDto);
                }
                else
                {
                    product.Variants.Add(variantDto.ToModel());
                }
            }
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
    }
}
