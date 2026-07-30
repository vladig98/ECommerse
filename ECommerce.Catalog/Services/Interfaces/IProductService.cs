namespace ECommerce.Catalog.Services.Interfaces;

public interface IProductService
{
    Task<ApiResponse<ProductDto>> CreateAsync(string username, CreateProductDto dto, CancellationToken token);
    Task<ApiResponse<ProductDto>> GetAsync(string username, Guid id, CancellationToken token);
    Task<ApiResponse<List<ProductDto>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<ProductDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token);
    Task<ApiResponse<ProductDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
