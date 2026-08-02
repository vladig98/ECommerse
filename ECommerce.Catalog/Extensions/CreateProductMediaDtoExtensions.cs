namespace ECommerce.Catalog.Extensions;

public static class CreateProductMediaDtoExtensions
{
    extension(CreateProductMediaDto createProductMediaDto)
    {
        public ProductMedia ToModel()
        {
            return new ProductMedia()
            {
                AltText = createProductMediaDto.AltText,
                DisplayOrder = createProductMediaDto.DisplayOrder,
                IsPrimary = createProductMediaDto.IsPrimary,
                Type = createProductMediaDto.Type,
                Url = createProductMediaDto.Url
            };
        }
    }
}
