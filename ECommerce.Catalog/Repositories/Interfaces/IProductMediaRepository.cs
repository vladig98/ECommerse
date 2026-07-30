namespace ECommerce.Catalog.Repositories.Interfaces;

public interface IProductMediaRepository
{
    Task<ApiResponse<List<ProductMedia>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<ProductMedia>> GetAsync(string username, Guid id, CancellationToken token);
    ApiResponse<ProductMedia> Create(string username, CreateProductMediaDto dto);
    Task<ApiResponse<ProductMedia>> UpdateAsync(string username, Guid id, Guid version, UpdateProductMediaDto dto, CancellationToken token);
    Task<ApiResponse<ProductMedia>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
