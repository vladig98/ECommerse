namespace ECommerce.Catalog.Services;

public class CachedVariantAttributeService([FromKeyedServices(KeyedServices.AttributeService)] IVariantAttributeService variantService, HybridCache hybridCache, ILogger logger) : IVariantAttributeService
{
    private static readonly CompositeFormat CategoryAttributeFormat = CompositeFormat.Parse(CacheKeys.AttributeKey);
    private static readonly CompositeFormat PaginatedAttributesFormat = CompositeFormat.Parse(CacheKeys.PaginatedAttributes);

    public async Task<ApiResponse<VariantAttributeDto>> CreateAsync(string username, CreateVariantAttributeDto dto, CancellationToken token)
    {
        ApiResponse<VariantAttributeDto> response = await variantService.CreateAsync(username, dto, token);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        VariantAttributeDto attributeDto = response.Data!;

        try
        {
            await hybridCache.SetAsync(string.Format(null, CategoryAttributeFormat, attributeDto.Id), attributeDto, cancellationToken: token);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllAttributesKey, cancellationToken: token);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Attribute '{attributeId}'", attributeDto.Id);
        }

        return response;
    }

    public async Task<ApiResponse<VariantAttributeDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        ApiResponse<VariantAttributeDto> response = await variantService.DeleteAsync(username, id, version, token);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        VariantAttributeDto attributeDto = response.Data!;

        try
        {
            await hybridCache.RemoveAsync(string.Format(null, CategoryAttributeFormat, attributeDto.Id), cancellationToken: token);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllAttributesKey, cancellationToken: token);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Attribute '{attributeId}'", attributeDto.Id);
        }

        return response;
    }

    public async Task<ApiResponse<PagedResult<VariantAttributeDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        string cacheKey = string.Format(null, PaginatedAttributesFormat, pageNumber, itemsPerPage);

        ApiResponse<PagedResult<VariantAttributeDto>> response = await hybridCache.GetOrCreateAsync(
            key: cacheKey,
            factory: async (ct) => await variantService.GetAllAsync(username, pageNumber, itemsPerPage, ct),
            options: null,
            tags: [CacheKeys.AllAttributesKey],
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

    public async Task<ApiResponse<VariantAttributeDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        string cacheKey = string.Format(null, CategoryAttributeFormat, id);

        ApiResponse<VariantAttributeDto> response = await hybridCache.GetOrCreateAsync(
            key: cacheKey,
            factory: async (ct) => await variantService.GetAsync(username, id, ct),
            cancellationToken: token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            try
            {
                await hybridCache.RemoveAsync(cacheKey, token);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Failed to evict failed response from cache for Attribute '{AttributeId}'", id);
            }
        }

        return response;
    }

    public async Task<ApiResponse<VariantAttributeDto>> UpdateAsync(string username, Guid id, Guid version, UpdateVariantAttributeDto dto, CancellationToken token)
    {
        ApiResponse<VariantAttributeDto> response = await variantService.UpdateAsync(username, id, version, dto, token);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        VariantAttributeDto attributeDto = response.Data!;

        try
        {
            await hybridCache.SetAsync(string.Format(null, CategoryAttributeFormat, attributeDto.Id), attributeDto, cancellationToken: token);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllAttributesKey, cancellationToken: token);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Attribute '{attributeId}'", attributeDto.Id);
        }

        return response;
    }
}