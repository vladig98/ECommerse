namespace ECommerce.Catalog.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<CategoryDto>> CreateAsync(string username, CreateCategoryDto dto, CancellationToken token);
    Task<ApiResponse<CategoryDto>> GetAsync(string username, Guid id, CancellationToken token);
    Task<ApiResponse<List<CategoryDto>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<CategoryDto>> UpdateAsync(string username, Guid id, Guid version, UpdateCategoryDto dto, CancellationToken token);
    Task<ApiResponse<CategoryDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
