namespace ECommerce.Catalog.Services;

public class CachedProductService([FromKeyedServices(KeyedServices.ProductService)] IProductService productService, HybridCache hybridCache, ILogger logger) : IProductService
{
    public async Task<ApiResponse<ProductDto>> CreateAsync(string username, CreateProductDto dto, CancellationToken token)
    {
        ApiResponse<ProductDto> response = await productService.CreateAsync(username, dto, token);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        ProductDto productDto = response.Data!;

        try
        {
            await hybridCache.SetAsync(string.Format(CacheKeys.ProductKey, productDto.Id), productDto, cancellationToken: token);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllProductsKey, cancellationToken: token);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Product '{ProductId}'", productDto.Id);
        }

        return response;
    }

    public async Task<ApiResponse<ProductDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        ApiResponse<ProductDto> response = await productService.DeleteAsync(username, id, version, token);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        ProductDto productDto = response.Data!;

        try
        {
            await hybridCache.RemoveAsync(string.Format(CacheKeys.ProductKey, productDto.Id), cancellationToken: token);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllProductsKey, cancellationToken: token);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Product '{ProductId}'", productDto.Id);
        }

        return response;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        string cacheKey = string.Format(CacheKeys.PaginatedProducts, pageNumber, itemsPerPage);

        ApiResponse<PagedResult<ProductDto>> response = await hybridCache.GetOrCreateAsync(
            key: cacheKey,
            factory: async (ct) => await productService.GetAllAsync(username, pageNumber, itemsPerPage, ct),
            options: null,
            tags: [CacheKeys.AllProductsKey],
            cancellationToken: token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            try
            {
                await hybridCache.RemoveAsync(cacheKey, token);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Failed to evict failed response from cache for key: {CacheKey}", cacheKey);
            }
        }

        return response;
    }

    public async Task<ApiResponse<ProductDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        string cacheKey = string.Format(CacheKeys.ProductKey, id);

        ApiResponse<ProductDto> response = await hybridCache.GetOrCreateAsync(
            key: cacheKey,
            factory: async (ct) => await productService.GetAsync(username, id, ct),
            cancellationToken: token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            try
            {
                await hybridCache.RemoveAsync(cacheKey, token);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Failed to evict failed response from cache for Product '{ProductId}'", id);
            }
        }

        return response;
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token)
    {
        ApiResponse<ProductDto> response = await productService.UpdateAsync(username, id, version, dto, token);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        ProductDto productDto = response.Data!;

        try
        {
            await hybridCache.SetAsync(string.Format(CacheKeys.ProductKey, productDto.Id), productDto, cancellationToken: token);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllProductsKey, cancellationToken: token);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Product '{ProductId}'", productDto.Id);
        }

        return response;
    }
}
