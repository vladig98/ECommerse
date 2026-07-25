namespace ECommerse.Identity.Data;

public class MainDbContext(DbContextOptions<MainDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
}
