namespace ECommerce.Catalog.Services;

internal class CachedProductService([FromKeyedServices(KeyedServices.ProductService)] IProductService productService, HybridCache hybridCache, ILogger logger) : IProductService
{
    private static readonly CompositeFormat ProductKeyFormat = CompositeFormat.Parse(CacheKeys.ProductKey);
    private static readonly CompositeFormat PaginatedProductsFormat = CompositeFormat.Parse(CacheKeys.PaginatedProducts);

    public async Task<ApiResponse<ProductDto>> CreateAsync(string username, CreateProductDto dto, CancellationToken token)
    {
        ApiResponse<ProductDto> response = await productService.CreateAsync(username, dto, token).ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        ProductDto productDto = response.Data!;

        try
        {
            await hybridCache.SetAsync(string.Format(null, ProductKeyFormat, productDto.Id), productDto, cancellationToken: token).ConfigureAwait(true);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllProductsKey, cancellationToken: token).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Product '{ProductId}'", productDto.Id);
        }

        return response;
    }

    public async Task<ApiResponse<ProductDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        ApiResponse<ProductDto> response = await productService.DeleteAsync(username, id, version, token).ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        ProductDto productDto = response.Data!;

        try
        {
            await hybridCache.RemoveAsync(string.Format(null, ProductKeyFormat, productDto.Id), cancellationToken: token).ConfigureAwait(true);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllProductsKey, cancellationToken: token).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Product '{ProductId}'", productDto.Id);
        }

        return response;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        string cacheKey = string.Format(null, PaginatedProductsFormat, pageNumber, itemsPerPage);

        ApiResponse<PagedResult<ProductDto>> response = await hybridCache.GetOrCreateAsync(
            key: cacheKey,
            factory: async (ct) => await productService.GetAllAsync(username, pageNumber, itemsPerPage, ct).ConfigureAwait(true),
            options: null,
            tags: [CacheKeys.AllProductsKey],
            cancellationToken: token)
            .ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            try
            {
                await hybridCache.RemoveAsync(cacheKey, token).ConfigureAwait(true);
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
        string cacheKey = string.Format(null, ProductKeyFormat, id);

        ApiResponse<ProductDto> response = await hybridCache.GetOrCreateAsync(
            key: cacheKey,
            factory: async (ct) => await productService.GetAsync(username, id, ct).ConfigureAwait(true),
            cancellationToken: token)
            .ConfigureAwait(true);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            try
            {
                await hybridCache.RemoveAsync(cacheKey, token).ConfigureAwait(true);
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
        ApiResponse<ProductDto> response = await productService.UpdateAsync(username, id, version, dto, token).ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        ProductDto productDto = response.Data!;

        try
        {
            await hybridCache.SetAsync(string.Format(null, ProductKeyFormat, productDto.Id), productDto, cancellationToken: token).ConfigureAwait(true);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllProductsKey, cancellationToken: token).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Product '{ProductId}'", productDto.Id);
        }

        return response;
    }
}
