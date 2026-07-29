namespace ECommerce.Catalog.Orchestrators;

public class ProductCreationOrchestrator(
    IProductsService productsService,
    IProductMediaService productMediaService,
    IProductVariantService productVariantService,
    IVariantAttributeService variantAttributeService,
    MainDbContext dbContext)
{
    public async Task<ApiResponse<ProductDto>> ExecuteAsync(string username, CreateProductDto dto, CancellationToken token)
    {
        ApiResponse<ProductDto> response = productsService.Create(username, dto);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        await productsService.AttachCategoryAsync(username, response.Data.Id, dto.CategoryId, token);

        foreach (CreateProductMediaDto mediaDto in dto.ProductMedia)
        {
            ApiResponse<ProductMediaDto> mediaResponse = productMediaService.Create(username, mediaDto);
            if (!string.IsNullOrWhiteSpace(mediaResponse.Error))
            {
                return new ApiResponse<ProductDto>(Code: mediaResponse.Code, Error: mediaResponse.Error);
            }

            await productsService.AttachMediaAsync(username, response.Data.Id, mediaResponse.Data.Id, token);
        }

        foreach (CreateProductVariantDto variantDto in dto.ProductVariants)
        {
            ApiResponse<ProductVariantDto> variantResponse = productVariantService.Create(username, variantDto);
            if (!string.IsNullOrWhiteSpace(variantResponse.Error))
            {
                return new ApiResponse<ProductDto>(Code: variantResponse.Code, Error: variantResponse.Error);
            }

            await productsService.AttachVariantAsync(username, response.Data.Id, variantResponse.Data.Id, token);

            foreach (CreateProductMediaDto mediaDto in variantDto.Media)
            {
                ApiResponse<ProductMediaDto> mediaResponse = productMediaService.Create(username, mediaDto);
                if (!string.IsNullOrWhiteSpace(mediaResponse.Error))
                {
                    return new ApiResponse<ProductDto>(Code: mediaResponse.Code, Error: mediaResponse.Error);
                }

                await productVariantService.AttachMediaAsync(username, variantResponse.Data.Id, mediaResponse.Data.Id, token);
            }

            foreach (CreateVariantAttributeDto attributeDto in variantDto.Attributes)
            {
                ApiResponse<VariantAttributeDto> attrResponse = variantAttributeService.Create(username, attributeDto);
                if (!string.IsNullOrWhiteSpace(attrResponse.Error))
                {
                    return new ApiResponse<ProductDto>(Code: attrResponse.Code, Error: attrResponse.Error);
                }

                await productVariantService.AttachAttributeAsync(username, variantResponse.Data.Id, attrResponse.Data.Id, token);
            }
        }

        await dbContext.SaveChangesAsync(token);

        return response;
    }
}
