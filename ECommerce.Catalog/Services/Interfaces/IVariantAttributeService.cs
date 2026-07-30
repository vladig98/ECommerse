namespace ECommerce.Catalog.Services.Interfaces;

public interface IVariantAttributeService
{
    Task<ApiResponse<VariantAttributeDto>> CreateAsync(string username, CreateVariantAttributeDto dto, CancellationToken token);
    Task<ApiResponse<VariantAttributeDto>> GetAsync(string username, Guid id, CancellationToken token);
    Task<ApiResponse<List<VariantAttributeDto>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<VariantAttributeDto>> UpdateAsync(string username, Guid id, Guid version, UpdateVariantAttributeDto dto, CancellationToken token);
    Task<ApiResponse<VariantAttributeDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
