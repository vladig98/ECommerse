using Microsoft.EntityFrameworkCore;

namespace ProductCatalogService.Data
{
    public class ProductsDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
        : base(options)
        {
        }
    }
}
