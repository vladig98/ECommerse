namespace ECommerce.Identity.Data;

public class MainDbContext(DbContextOptions<MainDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
}
