namespace ECommerce.Catalog.Repositories.Interfaces;

public interface IProductsRepository
{
    Task<ApiResponse<List<Product>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<Product>> GetAsync(string username, Guid id, CancellationToken token);
    ApiResponse<Product> Create(string username, CreateProductDto dto);
    Task<ApiResponse<Product>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token);
    Task<ApiResponse<Product>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
