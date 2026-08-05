namespace ECommerce.Catalog.Extensions;

internal static class ProductDbSetExtensions
{
    extension(DbSet<Product> products)
    {
        public IQueryable<Product> GetAllRelatedEntities()
        {
            return products.
                Include(x => x.Variants)
                    .ThenInclude(x => x.Media)
                .Include(x => x.Variants)
                    .ThenInclude(x => x.VariantAttributes)
                        .ThenInclude(x => x.Attribute)
                .Include(x => x.Category)
                    .ThenInclude(x => x.SubCategories)
                .Include(x => x.Media);
        }
    }
}
