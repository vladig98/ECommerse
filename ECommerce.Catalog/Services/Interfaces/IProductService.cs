namespace ECommerce.Catalog.Services.Interfaces;

internal interface IProductService
{
    Task<ApiResponse<ProductDto>> CreateAsync(string username, CreateProductDto dto, CancellationToken token);
    Task<ApiResponse<ProductDto>> GetAsync(string username, Guid id, CancellationToken token);
    Task<ApiResponse<PagedResult<ProductDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default);
    Task<ApiResponse<ProductDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token);
    Task<ApiResponse<ProductDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
