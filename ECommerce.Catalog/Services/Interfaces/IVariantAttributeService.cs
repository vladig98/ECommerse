namespace ECommerce.Catalog.Services.Interfaces;

public interface IVariantAttributeService
{
    Task<ApiResponse<IEnumerable<VariantAttributeDto>>> GetAllAsync(string username, CancellationToken token);
    Task<ApiResponse<VariantAttributeDto>> GetAsync(string username, Guid id, CancellationToken token);
    ApiResponse<VariantAttributeDto> Create(string username, CreateVariantAttributeDto dto);
    Task<ApiResponse<VariantAttributeDto>> UpdateAsync(string username, Guid id, Guid version, UpdateVariantAttributeDto dto, CancellationToken token);
    Task<ApiResponse<VariantAttributeDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
