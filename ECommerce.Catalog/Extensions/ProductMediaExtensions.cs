namespace ECommerce.Catalog.Extensions;

public static class ProductMediaExtensions
{
    extension(ProductMedia productMedia)
    {
        public void Update(UpdateProductMediaDto updateProductMediaDto)
        {
            productMedia.AltText = updateProductMediaDto.AltText;
            productMedia.DisplayOrder = updateProductMediaDto.DisplayOrder;
            productMedia.IsPrimary = updateProductMediaDto.IsPrimary;
            productMedia.Type = updateProductMediaDto.Type;
            productMedia.Url = updateProductMediaDto.Url;
        }

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

        public ProductMediaEventDto ToEventData()
        {
            return new ProductMediaEventDto
            (
                Id: productMedia.Id,
                Url: productMedia.Url,
                AltText: productMedia.AltText,
                Type: productMedia.Type,
                DisplayOrder: productMedia.DisplayOrder,
                IsPrimary: productMedia.IsPrimary
            );
        }
    }
}
