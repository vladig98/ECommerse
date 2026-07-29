namespace ECommerce.Catalog.Services.Interfaces;

public interface IProductVariantService
{
    Task<ApiResponse<IEnumerable<ProductVariantDto>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<ProductVariantDto>> GetAsync(string username, Guid id, CancellationToken token);
    ApiResponse<ProductVariantDto> Create(string username, CreateProductVariantDto dto);
    Task<ApiResponse<ProductVariantDto>> UpdateAsync(string username, Guid id, Guid version, UpdateProductVariantDto dto, CancellationToken token);
    Task<ApiResponse<ProductVariantDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
    Task AttachMediaAsync(string username, Guid variantId, Guid mediaId, CancellationToken token);
    Task AttachAttributeAsync(string username, Guid variantId, Guid attributeId, CancellationToken token);
}
