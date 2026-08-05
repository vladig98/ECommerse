namespace ECommerce.Catalog.Services.Interfaces;

internal interface IVariantAttributeService
{
    Task<ApiResponse<VariantAttributeDto>> CreateAsync(string username, CreateVariantAttributeDto dto, CancellationToken token);
    Task<ApiResponse<VariantAttributeDto>> GetAsync(string username, Guid id, CancellationToken token);
    Task<ApiResponse<PagedResult<VariantAttributeDto>>> GetAllAsync(string username, int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default);
    Task<ApiResponse<VariantAttributeDto>> UpdateAsync(string username, Guid id, Guid version, UpdateVariantAttributeDto dto, CancellationToken token);
    Task<ApiResponse<VariantAttributeDto>> DeleteAsync(string username, Guid id, Guid version, CancellationToken token);
}
