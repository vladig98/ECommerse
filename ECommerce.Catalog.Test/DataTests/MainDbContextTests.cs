namespace ECommerce.Catalog.Test.DataTests;

public class MainDbContextTests : IDisposable
{
    private readonly MainDbContext mainDbContext;
    private bool isDisposed;
    private IntPtr nativeResource = Marshal.AllocHGlobal(100);

    public MainDbContextTests()
    {
        DbContextOptions<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>()
           .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
           .Options;

        mainDbContext = new(options);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            mainDbContext.Dispose();
        }

        if (nativeResource != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(nativeResource);
            nativeResource = IntPtr.Zero;
        }

        isDisposed = true;
    }

    [Fact]
    public void TestEnsureDBHasCitextExtensionEnabled()
    {
        IReadOnlyList<PostgresExtension> extensions = mainDbContext.Model.GetPostgresExtensions();
        FieldInfo? field = typeof(PostgresExtension).GetField("_annotationName", BindingFlags.NonPublic | BindingFlags.Instance);

        bool hasCitext = extensions.Any(ext => field?.GetValue(ext) is string annotationName && annotationName.Contains("citext", StringComparison.Ordinal));
        Assert.True(hasCitext, "The 'citext' PostgreSQL extension is not configured.");
    }

    [Fact]
    public void TestEnsureEveryModelHasDedicatedEntityTypeConfiguration()
    {
        List<Type> modelTypes = [.. typeof(BaseModel).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(BaseModel).IsAssignableFrom(t))];

        List<Type> configurationInterfaces = [.. typeof(MainDbContext).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))];

        HashSet<Type> configuredEntityTypes = [.. configurationInterfaces.Select(i => i.GenericTypeArguments[0])];

        foreach (Type modelType in modelTypes)
        {
            Assert.True(
                configuredEntityTypes.Contains(modelType),
                $"Domain model '{modelType.Name}' is missing a dedicated IEntityTypeConfiguration<{modelType.Name}> class."
            );
        }
    }

    [Fact]
    public void TestEnsureAllEntitiesHaveExplicitTableNames()
    {
        IEnumerable<IEntityType> entityTypes = mainDbContext.Model.GetEntityTypes();

        foreach (IEntityType entity in entityTypes)
        {
            string? tableName = entity.GetTableName();
            Assert.NotNull(tableName);
        }
    }

    [Fact]
    public void TestEnsureNoUnintendedShadowProperties()
    {
        IEnumerable<IProperty> shadowProperties = mainDbContext.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.IsShadowProperty() && p.Name != "PeriodStart" && p.Name != "PeriodEnd");

        Assert.Empty(shadowProperties);
    }

    [Fact]
    public void TestEnsureOnlyBaseModelsAreInDBSets()
    {
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
    public void TestEnsureAllBaseModelsAreInDBSets()
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
