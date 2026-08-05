namespace ECommerce.Catalog.Repositories.Interfaces;

internal interface ICategoryRepository
{
    Task<PagedResult<Category>> GetAllAsync(int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default);
    Task<Category?> GetAsync(Guid id, CancellationToken token);
    Task<Category> AddAsync(Category category, CancellationToken token);
    Task UpdateAsync(Category category, Guid version, CancellationToken token);
    Task<Category?> DeleteAsync(Guid id, Guid version, CancellationToken token);
}
