namespace ECommerce.Catalog.Extensions;

public static class UpdateProductVariantDtoExtensions
{
    extension(UpdateProductVariantDto updateProductVariantDto)
    {
        public ProductVariant ToModel()
        {
            return new ProductVariant()
            {
                BasePrice = updateProductVariantDto.BasePrice,
                Gtin = updateProductVariantDto.Gtin,
                Sku = updateProductVariantDto.Sku,
                Media = [.. updateProductVariantDto.Media.Select(x => x.ToModel())],
                VariantAttributes = [.. updateProductVariantDto.Attributes.Select(x => new ProductVariantAttribute() { AttributeId = x })]
            };
        }
    }
}
