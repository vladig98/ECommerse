namespace ECommerce.Catalog.Services.Interfaces;

public interface IProductsService
{
    Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<ProductDto>> GetAsync(string username, Guid id, CancellationToken token);
    ApiResponse<ProductDto> Create(string username, CreateProductDto dto);
    Task<ApiResponse<ProductDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductDto dto, CancellationToken token);
    Task<ApiResponse<ProductDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
    Task AttachCategoryAsync(string username, Guid productId, Guid categoryId, CancellationToken token);
    Task AttachMediaAsync(string username, Guid productId, Guid mediaId, CancellationToken token);
    Task AttachVariantAsync(string username, Guid productId, Guid variantId, CancellationToken token);
}
