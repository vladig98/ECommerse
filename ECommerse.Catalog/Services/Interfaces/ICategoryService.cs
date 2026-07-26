namespace ECommerse.Catalog.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<CategoryDto>> GetAsync(string username, string id, CancellationToken token);
    Task<ApiResponse<CategoryDto>> CreateAsync(string username, CreateCategoryDto dto, CancellationToken token);
    Task<ApiResponse<CategoryDto>> UpdateAsync(string username, string id, Guid version, UpdateCategoryDto dto, CancellationToken token);
    Task<ApiResponse<CategoryDto>> DeleteAsync(string username, string id, Guid version, CancellationToken token);
}
