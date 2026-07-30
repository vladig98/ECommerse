namespace ECommerce.Catalog.Extensions;

public static class WebBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddDatabase()
        {
            string connectionString = builder.Configuration.GetConnectionString("Main")
                ?? throw new InvalidOperationException("Missing connection string.");

            builder.Services.AddDbContext<MainDbContext>(options =>
                options.UseNpgsql(connectionString));

            return builder;
        }

        public WebApplicationBuilder MapSettings()
        {
            builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
            return builder;
        }

        public WebApplicationBuilder ConfigureRouting()
        {
            builder.Services.AddOpenApi();

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            return builder;
        }

        public WebApplicationBuilder ConfigureSerilog()
        {
            builder.Services.AddSerilog();
            builder.Services.AddSingleton(Log.Logger);

            return builder;
        }

        public WebApplicationBuilder ConfigureAuth()
        {
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            return builder;
        }

        public WebApplicationBuilder RegisterApplicationServices()
        {
            // Repositories
            builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductMediaRepository, ProductMediaRepository>();
            builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            builder.Services.AddScoped<IVariantAttributeRepository, VariantAttributeRepository>();

            // Services
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IVariantAttributeService, VariantAttributeService>();

            // Background service
            builder.Services.AddHostedService<KafkaEventProducer>();

            return builder;
        }
    }
}
