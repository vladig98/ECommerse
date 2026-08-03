namespace ECommerce.Catalog.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<CategoryDto>> CreateAsync(string username, CreateCategoryDto dto, CancellationToken token);
    Task<ApiResponse<CategoryDto>> GetAsync(string username, Guid id, CancellationToken token);
    Task<ApiResponse<PagedResult<CategoryDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default);
    Task<ApiResponse<CategoryDto>> UpdateAsync(string username, Guid id, Guid version, UpdateCategoryDto dto, CancellationToken token);
    Task<ApiResponse<CategoryDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
