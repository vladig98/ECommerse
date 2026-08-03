namespace ECommerce.Catalog.Repositories;

public class VariantAttributeRepository(MainDbContext dbContext, ILogger logger) : IVariantAttributeRepository
{
    public async Task<VariantAttribute> AddAsync(VariantAttribute attribute, CancellationToken token)
    {
        logger.Debug("Executing INSERT for new Variant Attribute in database.");

        dbContext.VariantAttributes.Add(attribute);
        await dbContext.SaveChangesAsync(token);

        VariantAttribute? createdAttribute = await GetAsync(attribute.Id, token);

        return createdAttribute!;
    }

    public async Task<VariantAttribute?> DeleteAsync(Guid id, Guid version, CancellationToken token)
    {
        VariantAttribute? attribute = await GetAsync(id, token);
        if (attribute is null)
        {
            return attribute;
        }

        logger.Debug("Executing DELETE for Variant Attribute '{AttributeId}' in database.", id);

        dbContext.Entry(attribute).Property(p => p.Version).OriginalValue = version;
        dbContext.VariantAttributes.Remove(attribute);

        await dbContext.SaveChangesAsync(token);

        return attribute;
    }

    public async Task<PagedResult<VariantAttribute>> GetAllAsync(int pageNumber = 1, int itemsPerPage = 100, CancellationToken token = default)
    {
        logger.Debug("Executing COUNT and SELECT for paginated Variant Attributes.");

        int totalCount = await dbContext.VariantAttributes.CountAsync(token);
        int totalPages = (int)Math.Ceiling(totalCount / (double)itemsPerPage);

        int realPageNumber = Math.Clamp(pageNumber - 1, 0, totalPages);
        int itemsToSkip = realPageNumber * itemsPerPage;

        List<VariantAttribute> items = await dbContext.VariantAttributes
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip(itemsToSkip)
            .Take(itemsPerPage)
            .ToListAsync(token);

        return new PagedResult<VariantAttribute>(items, totalCount, pageNumber, itemsPerPage, totalPages);
    }

    public async Task<VariantAttribute?> GetAsync(Guid id, CancellationToken token)
    {
        logger.Debug("Executing SELECT for Variant Attribute '{AttributeId}'.", id);

        return await dbContext.VariantAttributes
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);
    }

    public async Task UpdateAsync(VariantAttribute attribute, Guid version, CancellationToken token)
    {
        logger.Debug("Executing UPDATE for Variant Attribute '{AttributeId}' in database.", attribute.Id);

        dbContext.Entry(attribute).Property(p => p.Version).OriginalValue = version;
        await dbContext.SaveChangesAsync(token);
    }
}