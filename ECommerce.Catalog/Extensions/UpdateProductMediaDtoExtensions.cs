namespace ECommerce.Catalog.Extensions;

internal static class UpdateProductMediaDtoExtensions
{
    extension(UpdateProductMediaDto updateProductMediaDto)
    {
        public ProductMedia ToModel()
        {
            return new ProductMedia()
            {
                AltText = updateProductMediaDto.AltText,
                DisplayOrder = updateProductMediaDto.DisplayOrder,
                IsPrimary = updateProductMediaDto.IsPrimary,
                Type = updateProductMediaDto.Type,
                Url = updateProductMediaDto.Url
            };
        }
    }
}
