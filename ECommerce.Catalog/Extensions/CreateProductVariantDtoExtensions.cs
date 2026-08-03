namespace ECommerce.Catalog.Extensions;

public static class CreateProductVariantDtoExtensions
{
    extension(CreateProductVariantDto createProductVariantDto)
    {
        public ProductVariant ToModel()
        {
            return new ProductVariant()
            {
                BasePrice = createProductVariantDto.BasePrice,
                Gtin = createProductVariantDto.Gtin,
                Sku = createProductVariantDto.Sku,
                Media = [.. createProductVariantDto.Media.Select(x => x.ToModel())],
                VariantAttributes = [.. createProductVariantDto.Attributes.Select(x => new ProductVariantAttribute() { AttributeId = x })]
            };
        }
    }
}
