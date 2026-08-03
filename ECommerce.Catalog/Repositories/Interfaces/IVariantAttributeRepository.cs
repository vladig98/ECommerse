namespace ECommerce.Catalog.Repositories.Interfaces;

public interface IVariantAttributeRepository
{
    Task<PagedResult<VariantAttribute>> GetAllAsync(int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default);
    Task<VariantAttribute?> GetAsync(Guid id, CancellationToken token);
    Task<VariantAttribute> AddAsync(VariantAttribute attribute, CancellationToken token);
    Task UpdateAsync(VariantAttribute attribute, Guid version, CancellationToken token);
    Task<VariantAttribute?> DeleteAsync(Guid id, Guid version, CancellationToken token);
}
