namespace ECommerce.Catalog.Extensions;

public static class CreateProductDtoExtensions
{
    extension(CreateProductDto createProductDto)
    {
        public Product ToModel()
        {
            return new Product()
            {
                Brand = createProductDto.Brand,
                CategoryId = createProductDto.CategoryId,
                Description = createProductDto.Description,
                IsActive = createProductDto.IsActive,
                Slug = createProductDto.Slug,
                Title = createProductDto.Title,
                Media = [.. createProductDto.ProductMedia.Select(x => x.ToModel())],
                Variants = [.. createProductDto.ProductVariants.Select(x => x.ToModel())]
            };
        }
    }
}
