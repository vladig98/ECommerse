namespace ECommerce.Catalog.Services;

public class ProductService(
    IProductsRepository productsService,
    ILogger logger) : IProductService
{
    public async Task<ApiResponse<ProductDto>> CreateAsync(string username, CreateProductDto dto, CancellationToken token)
    {
        ApiResponse<Product> productResponse = await productsService.CreateAsync(username, dto, token);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Aborting product creation: Failed to create root product. User: '{Username}'", username);
            return ApiResponse<ProductDto>.FromResponse(productResponse);
        }

        Product product = productResponse.Data!;
        ProductDto productDto = product.ToDto();

        return ApiResponse<ProductDto>.Success(productDto);
    }

    public async Task<ApiResponse<ProductDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        ApiResponse<Product> productResponse = await productsService.DeleteAsync(username, id, version, token);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Aborting product deletion: Failed to prepare product '{ProductId}' for deletion. User: '{Username}'. Reason: {Error}", id, username, productResponse.Error);
            return ApiResponse<ProductDto>.FromResponse(productResponse);
        }

        Product product = productResponse.Data!;
        ProductDto dto = product.ToDto();

        return ApiResponse<ProductDto>.Success(dto);
    }

    public async Task<ApiResponse<List<ProductDto>>> GetAllAsync(string username, CancellationToken token)
    {
        ApiResponse<List<Product>> productResponse = await productsService.GetAllAsync(username, token);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Failed to retrieve products. User: '{Username}'. Reason: {Error}", username, productResponse.Error);
            return ApiResponse<List<ProductDto>>.FromResponse(productResponse);
        }

        List<Product> products = productResponse.Data!;
        List<ProductDto> dtos = [.. products.Select(x => x.ToDto())];

        return ApiResponse<List<ProductDto>>.Success(dtos);
    }

    public async Task<ApiResponse<ProductDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        ApiResponse<Product> productResponse = await productsService.GetAsync(username, id, token);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Failed to retrieve product '{ProductId}'. User: '{Username}'. Reason: {Error}", id, username, productResponse.Error);
            return ApiResponse<ProductDto>.FromResponse(productResponse);
        }

        Product product = productResponse.Data!;
        ProductDto dto = product.ToDto();

        return ApiResponse<ProductDto>.Success(dto);
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token)
    {
        ApiResponse<Product> productResponse = await productsService.UpdateAsync(username, id, version, dto, token);
        if (!string.IsNullOrWhiteSpace(productResponse.Error))
        {
            logger.Warning("Aborting product update: Failed to update root product '{ProductId}'. User: '{Username}'", id, username);
            return ApiResponse<ProductDto>.FromResponse(productResponse);
        }

        Product product = productResponse.Data!;
        ProductDto productDto = product.ToDto();

        return ApiResponse<ProductDto>.Success(productDto);
    }
}