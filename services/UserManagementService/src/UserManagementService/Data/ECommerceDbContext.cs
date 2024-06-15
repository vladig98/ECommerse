using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedModels;

namespace UserManagementService.Data
{
    public class ECommerceDbContext : IdentityDbContext<User, Role, string>
    {
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options):base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasMany(x => x.PaymentMethods).WithOne(x => x.User).HasForeignKey(x => x.UserId);
            modelBuilder.Entity<User>().HasMany(x => x.Orders).WithOne(x => x.Client).HasForeignKey(x => x.ClientId);
            modelBuilder.Entity<User>().HasMany(x => x.Addresses).WithOne(x => x.User).HasForeignKey(x => x.UserId);

            modelBuilder.Entity<Order>().HasMany(x => x.OrderItems).WithOne(x => x.Order).HasForeignKey(x => x.OrderId);
            modelBuilder.Entity<Order>().HasOne(x => x.ShippingAddress).WithMany().HasForeignKey(x => x.ShippingAddressId);
            modelBuilder.Entity<Order>().HasOne(x => x.BillingAddress).WithMany().HasForeignKey(x => x.BillingAddressId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
