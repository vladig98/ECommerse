namespace ECommerse.Catalog.Services.Interfaces;

public interface IProductsService
{
    Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<ProductDto>> GetAsync(string username, string id, CancellationToken token);
    Task<ApiResponse<ProductDto>> CreateAsync(string username, CreateProductDto dto, CancellationToken token);
    Task<ApiResponse<ProductDto>> UpdateAsync(string username, string id, Guid version, UpdateProductDto dto, CancellationToken token);
    Task<ApiResponse<ProductDto>> DeleteAsync(string username, string id, Guid version, CancellationToken token);
}
