namespace ECommerce.Catalog.Test.DataTests;

public class MainDbContextTests : IDisposable
{
    private readonly MainDbContext mainDbContext;

    public MainDbContextTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
           .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
           .Options;

        mainDbContext = new(options);
    }

    public void Dispose()
    {
        mainDbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Test_Ensure_Only_Base_Models_Are_In_DB_Sets()
    {
        // Exclude the mapping tables that EF core creates automatically
        Type[] props = [.. mainDbContext.GetType().GetProperties()
            .Select(x => x.PropertyType)
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(x => x.GenericTypeArguments[0])];

        foreach (Type dbSet in props)
        {
            Assert.True(typeof(BaseModel).IsAssignableFrom(dbSet));
        }
    }

    [Fact]
    public void Test_Ensure_All_Base_Models_Are_In_DB_Sets()
    {
        Type[] models = [.. typeof(BaseModel).Assembly.GetTypes()
            .Where(type => typeof(BaseModel).IsAssignableFrom(type) && !type.IsAbstract && type.IsClass)];

        HashSet<Type> types = [.. mainDbContext.Model.GetEntityTypes().Select(x => x.ClrType)];

        foreach (Type model in models)
        {
            Assert.Contains(model, types);
        }
    }
}
