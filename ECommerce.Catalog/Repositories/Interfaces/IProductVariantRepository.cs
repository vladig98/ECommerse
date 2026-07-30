namespace ECommerce.Catalog.Repositories.Interfaces;

public interface IProductVariantRepository
{
    Task<ApiResponse<List<ProductVariant>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<ProductVariant>> GetAsync(string username, Guid id, CancellationToken token);
    ApiResponse<ProductVariant> Create(string username, CreateProductVariantDto dto);
    Task<ApiResponse<ProductVariant>> UpdateAsync(string username, Guid id, Guid version, UpdateProductVariantDto dto, CancellationToken token);
    Task<ApiResponse<ProductVariant>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
