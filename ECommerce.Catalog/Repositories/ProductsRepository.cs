namespace ECommerce.Catalog.Repositories;

internal class ProductsRepository(MainDbContext dbContext, ILogger logger) : IProductsRepository
{
    public async Task<Product> AddAsync(Product product, CancellationToken token)
    {
        logger.Debug("Executing INSERT for new Product in database.");

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(token).ConfigureAwait(true);

        Product? createdProduct = await GetAsync(product.Id, token).ConfigureAwait(true);

        return createdProduct!;
    }

    public async Task<Product?> DeleteAsync(Guid id, Guid version, CancellationToken token)
    {
        Product? product = await GetAsync(id, token).ConfigureAwait(true);
        if (product is null)
        {
            return product;
        }

        logger.Debug("Executing DELETE for Product '{ProductId}' in database.", id);

        dbContext.Entry(product).Property(p => p.Version).OriginalValue = version;
        dbContext.Products.Remove(product);

        await dbContext.SaveChangesAsync(token).ConfigureAwait(true);

        return product;
    }

    public async Task<PagedResult<Product>> GetAllAsync(int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        logger.Debug("Executing COUNT and SELECT for paginated Products.");

        int totalCount = await dbContext.Products.CountAsync(token).ConfigureAwait(true);
        int totalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage);

        int realPageNumber = Math.Clamp(pageNumber - 1, 0, totalPages);
        int itemsToSkip = realPageNumber * itemsPerPage;

        List<Product> items = await dbContext.Products
            .GetAllRelatedEntities()
            .AsSplitQuery()
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip(itemsToSkip)
            .Take(itemsPerPage)
            .ToListAsync(token)
            .ConfigureAwait(true);

        return new PagedResult<Product>(items, totalCount, pageNumber, itemsPerPage, totalPages);
    }

    public async Task<Product?> GetAsync(Guid id, CancellationToken token)
    {
        logger.Debug("Executing SELECT for Product '{ProductId}'.", id);

        return await dbContext.Products
                .GetAllRelatedEntities()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token)
                .ConfigureAwait(true);
    }

    public async Task UpdateAsync(Product product, Guid version, CancellationToken token)
    {
        logger.Debug("Executing UPDATE for Product '{ProductId}' in database.", product.Id);

        dbContext.Entry(product).State = EntityState.Modified;
        dbContext.Entry(product).Property(p => p.Version).OriginalValue = version;

        await dbContext.SaveChangesAsync(token).ConfigureAwait(true);
    }
}