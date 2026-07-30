namespace ECommerce.Catalog.Extensions;

public static class ProductMediaExtensions
{
    extension(ProductMedia productMedia)
    {
        public ProductMediaDto ToDto()
        {
            return new ProductMediaDto
            (
                Id: productMedia.Id,
                CreatedAt: productMedia.CreatedAt,
                UpdatedAt: productMedia.UpdatedAt,
                Version: productMedia.Version,
                Url: productMedia.Url,
                AltText: productMedia.AltText,
                Type: productMedia.Type,
                DisplayOrder: productMedia.DisplayOrder,
                IsPrimary: productMedia.IsPrimary
            );
        }
    }
}
