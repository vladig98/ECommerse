namespace ECommerce.Catalog.Services;

public class CachedCategoryService([FromKeyedServices(KeyedServices.CategoryService)] ICategoryService categoryService, HybridCache hybridCache, ILogger logger) : ICategoryService
{
    public async Task<ApiResponse<CategoryDto>> CreateAsync(string username, CreateCategoryDto dto, CancellationToken token)
    {
        ApiResponse<CategoryDto> response = await categoryService.CreateAsync(username, dto, token);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        CategoryDto categoryDto = response.Data!;

        try
        {
            await hybridCache.SetAsync(string.Format(CacheKeys.CategoryKey, categoryDto.Id), categoryDto, cancellationToken: token);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllCategoriesKey, cancellationToken: token);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Category '{CategoryId}'", categoryDto.Id);
        }

        return response;
    }

    public async Task<ApiResponse<CategoryDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token)
    {
        ApiResponse<CategoryDto> response = await categoryService.DeleteAsync(username, id, version, token);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        CategoryDto categoryDto = response.Data!;

        try
        {
            await hybridCache.RemoveAsync(string.Format(CacheKeys.CategoryKey, categoryDto.Id), cancellationToken: token);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllCategoriesKey, cancellationToken: token);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Category '{CategoryId}'", categoryDto.Id);
        }

        return response;
    }

    public async Task<ApiResponse<PagedResult<CategoryDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        string cacheKey = string.Format(CacheKeys.PaginatedCategories, pageNumber, itemsPerPage);

        ApiResponse<PagedResult<CategoryDto>> response = await hybridCache.GetOrCreateAsync(
            key: cacheKey,
            factory: async (ct) => await categoryService.GetAllAsync(username, pageNumber, itemsPerPage, ct),
            options: null,
            tags: [CacheKeys.AllCategoriesKey],
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

    public async Task<ApiResponse<CategoryDto>> GetAsync(string username, Guid id, CancellationToken token)
    {
        string cacheKey = string.Format(CacheKeys.CategoryKey, id);

        ApiResponse<CategoryDto> response = await hybridCache.GetOrCreateAsync(
            key: cacheKey,
            factory: async (ct) => await categoryService.GetAsync(username, id, ct),
            cancellationToken: token);

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            try
            {
                await hybridCache.RemoveAsync(cacheKey, token);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Failed to evict failed response from cache for Category '{CategoryId}'", id);
            }
        }

        return response;
    }

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(string username, Guid id, Guid version, UpdateCategoryDto dto, CancellationToken token)
    {
        ApiResponse<CategoryDto> response = await categoryService.UpdateAsync(username, id, version, dto, token);
        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response;
        }

        CategoryDto categoryDto = response.Data!;

        try
        {
            await hybridCache.SetAsync(string.Format(CacheKeys.CategoryKey, categoryDto.Id), categoryDto, cancellationToken: token);
            await hybridCache.RemoveByTagAsync(CacheKeys.AllCategoriesKey, cancellationToken: token);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Failed to update cache for Category '{CategoryId}'", categoryDto.Id);
        }

        return response;
    }
}