namespace ECommerce.Catalog.Repositories.Interfaces;

public interface IVariantAttributeRepository
{
    Task<ApiResponse<List<VariantAttribute>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<VariantAttribute>> GetAsync(string username, Guid id, CancellationToken token);
    ApiResponse<VariantAttribute> Create(string username, CreateVariantAttributeDto dto);
    Task<ApiResponse<VariantAttribute>> UpdateAsync(string username, Guid id, Guid version, UpdateVariantAttributeDto dto, CancellationToken token);
    Task<ApiResponse<VariantAttribute>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
