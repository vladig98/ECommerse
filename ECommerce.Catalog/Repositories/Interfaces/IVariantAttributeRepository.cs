namespace ECommerce.Catalog.Repositories.Interfaces;

internal interface IVariantAttributeRepository
{
    Task<PagedResult<VariantAttributeModel>> GetAllAsync(int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default);
    Task<VariantAttributeModel?> GetAsync(Guid id, CancellationToken token);
    Task<VariantAttributeModel> AddAsync(VariantAttributeModel attribute, CancellationToken token);
    Task UpdateAsync(VariantAttributeModel attribute, Guid version, CancellationToken token);
    Task<VariantAttributeModel?> DeleteAsync(Guid id, Guid version, CancellationToken token);
}
