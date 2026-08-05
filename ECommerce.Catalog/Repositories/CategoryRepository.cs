namespace ECommerce.Catalog.Repositories;

internal class CategoryRepository(MainDbContext dbContext, ILogger logger) : ICategoryRepository
{
    public async Task<Category> AddAsync(Category category, CancellationToken token)
    {
        logger.Debug("Executing INSERT for new Category in database.");

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(token).ConfigureAwait(true);

        Category? createdCategory = await GetAsync(category.Id, token).ConfigureAwait(true);

        return createdCategory!;
    }

    public async Task<Category?> DeleteAsync(Guid id, Guid version, CancellationToken token)
    {
        Category? category = await GetAsync(id, token).ConfigureAwait(true);
        if (category is null)
        {
            return category;
        }

        logger.Debug("Executing DELETE for Category '{CategoryId}' in database.", id);

        dbContext.Entry(category).Property(p => p.Version).OriginalValue = version;
        dbContext.Categories.Remove(category);

        await dbContext.SaveChangesAsync(token).ConfigureAwait(true);

        return category;
    }

    public async Task<PagedResult<Category>> GetAllAsync(int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        logger.Debug("Executing COUNT and SELECT for paginated Categories.");

        int totalCount = await dbContext.Categories.CountAsync(token).ConfigureAwait(true);
        int totalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage);

        int realPageNumber = Math.Clamp(pageNumber - 1, 0, totalPages);
        int itemsToSkip = realPageNumber * itemsPerPage;

        List<Category> items = await dbContext.Categories
            .GetAllRelatedEntities()
            .AsSplitQuery()
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip(itemsToSkip)
            .Take(itemsPerPage)
            .ToListAsync(token)
            .ConfigureAwait(true);

        return new PagedResult<Category>(items, totalCount, pageNumber, itemsPerPage, totalPages);
    }

    public async Task<Category?> GetAsync(Guid id, CancellationToken token)
    {
        logger.Debug("Executing SELECT for Category '{CategoryId}'.", id);

        return await dbContext.Categories
                .GetAllRelatedEntities()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token)
                .ConfigureAwait(true);
    }

    public async Task UpdateAsync(Category category, Guid version, CancellationToken token)
    {
        logger.Debug("Executing UPDATE for Category '{CategoryId}' in database.", category.Id);

        dbContext.Entry(category).Property(p => p.Version).OriginalValue = version;
        await dbContext.SaveChangesAsync(token).ConfigureAwait(true);
    }
}