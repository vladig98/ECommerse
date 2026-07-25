using Microsoft.EntityFrameworkCore;

namespace OrderManagementService.Data
{
    public class OrderManagementDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<PaymentDetails> PaymentDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

        public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options) : base(options)
        {
        }
    }
}
