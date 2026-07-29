namespace ECommerce.Catalog.Services.Interfaces;

public interface IProductMediaService
{
    Task<ApiResponse<IEnumerable<ProductMediaDto>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<ProductMediaDto>> GetAsync(string username, Guid id, CancellationToken token);
    ApiResponse<ProductMediaDto> Create(string username, CreateProductMediaDto dto);
    Task<ApiResponse<ProductMediaDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductMediaDto dto, CancellationToken token);
    Task<ApiResponse<ProductMediaDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
