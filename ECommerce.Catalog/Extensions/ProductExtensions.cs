namespace ECommerce.Catalog.Extensions;

public static class ProductExtensions
{
    extension(Product product)
    {
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
    }
}
