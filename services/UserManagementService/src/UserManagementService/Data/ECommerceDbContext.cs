using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserManagementService.Data
{
    public class ECommerceDbContext : IdentityDbContext<User, Role, string>
    {
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options) { }
    }
}
