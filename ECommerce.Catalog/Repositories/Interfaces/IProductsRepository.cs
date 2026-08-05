namespace ECommerce.Catalog.Repositories.Interfaces;

internal interface IProductsRepository
{
    Task<PagedResult<Product>> GetAllAsync(int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default);
    Task<Product?> GetAsync(Guid id, CancellationToken token);
    Task<Product> AddAsync(Product product, CancellationToken token);
    Task UpdateAsync(Product product, Guid version, CancellationToken token);
    Task<Product?> DeleteAsync(Guid id, Guid version, CancellationToken token);
}
