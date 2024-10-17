using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserManagementService.Data
{
    public class ECommerceDbContext : IdentityDbContext<User, Role, string>
    {
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserRole>().HasOne(x => x.User).WithMany(x => x.Roles).HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UserRole>().HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }
    }
}
