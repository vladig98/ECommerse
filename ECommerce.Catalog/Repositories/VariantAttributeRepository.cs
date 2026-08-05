namespace ECommerce.Catalog.Repositories;

internal class VariantAttributeRepository(MainDbContext dbContext, ILogger logger) : IVariantAttributeRepository
{
    public async Task<VariantAttributeModel> AddAsync(VariantAttributeModel attribute, CancellationToken token)
    {
        logger.Debug("Executing INSERT for new Variant Attribute in database.");

        dbContext.VariantAttributes.Add(attribute);
        await dbContext.SaveChangesAsync(token).ConfigureAwait(true);

        VariantAttributeModel? createdAttribute = await GetAsync(attribute.Id, token).ConfigureAwait(true);

        return createdAttribute!;
    }

    public async Task<VariantAttributeModel?> DeleteAsync(Guid id, Guid version, CancellationToken token)
    {
        VariantAttributeModel? attribute = await GetAsync(id, token).ConfigureAwait(true);
        if (attribute is null)
        {
            return attribute;
        }

        logger.Debug("Executing DELETE for Variant Attribute '{AttributeId}' in database.", id);

        dbContext.Entry(attribute).Property(p => p.Version).OriginalValue = version;
        dbContext.VariantAttributes.Remove(attribute);

        await dbContext.SaveChangesAsync(token).ConfigureAwait(true);

        return attribute;
    }

    public async Task<PagedResult<VariantAttributeModel>> GetAllAsync(int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        logger.Debug("Executing COUNT and SELECT for paginated Variant Attributes.");

        int totalCount = await dbContext.VariantAttributes.CountAsync(token).ConfigureAwait(true);
        int totalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage);

        int realPageNumber = Math.Clamp(pageNumber - 1, 0, totalPages);
        int itemsToSkip = realPageNumber * itemsPerPage;

        List<VariantAttributeModel> items = await dbContext.VariantAttributes
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip(itemsToSkip)
            .Take(itemsPerPage)
            .ToListAsync(token)
            .ConfigureAwait(true);

        return new PagedResult<VariantAttributeModel>(items, totalCount, pageNumber, itemsPerPage, totalPages);
    }

    public async Task<VariantAttributeModel?> GetAsync(Guid id, CancellationToken token)
    {
        logger.Debug("Executing SELECT for Variant Attribute '{AttributeId}'.", id);

        return await dbContext.VariantAttributes
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token)
                .ConfigureAwait(true);
    }

    public async Task UpdateAsync(VariantAttributeModel attribute, Guid version, CancellationToken token)
    {
        logger.Debug("Executing UPDATE for Variant Attribute '{AttributeId}' in database.", attribute.Id);

        dbContext.Entry(attribute).Property(p => p.Version).OriginalValue = version;
        await dbContext.SaveChangesAsync(token).ConfigureAwait(true);
    }
}