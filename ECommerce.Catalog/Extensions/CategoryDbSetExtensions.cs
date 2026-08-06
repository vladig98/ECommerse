namespace ECommerce.Catalog.Extensions;

public static class CategoryDbSetExtensions
{
    extension(DbSet<Category> categories)
    {
        public IQueryable<Category> GetAllRelatedEntities()
        {
            return categories
                .Include(x => x.ParentCategory)
                .Include(x => x.SubCategories);
        }
    }
}
