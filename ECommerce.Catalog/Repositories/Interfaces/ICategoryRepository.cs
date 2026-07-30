namespace ECommerce.Catalog.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<ApiResponse<List<Category>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<Category>> GetAsync(string username, Guid id, CancellationToken token);
    ApiResponse<Category> Create(string username, CreateCategoryDto dto);
    Task<ApiResponse<Category>> UpdateAsync(string username, Guid id, Guid version, UpdateCategoryDto dto, CancellationToken token);
    Task<ApiResponse<Category>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
