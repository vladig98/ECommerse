namespace ECommerce.Identity.Extentions;

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

        public WebApplicationBuilder SetUpIdentity()
        {
            builder.Services.AddIdentityCore<IdentityUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<MainDbContext>();

            builder.Services.AddTransient<IEmailSender<IdentityUser>, NoOpEmailSender>();

            return builder;
        }

        public WebApplicationBuilder MapConfigurationSettings()
        {
            IConfigurationSection jwtSection = builder.Configuration.GetSection("JwtConfig");
            IConfigurationSection openIdSection = builder.Configuration.GetSection("OpenIDConfig");

            builder.Services.Configure<JwtConfigSettings>(jwtSection);
            builder.Services.Configure<OpenIDConfigSettings>(openIdSection);

            return builder;
        }

        public WebApplicationBuilder SetupAuthentication()
        {
            JwtConfigSettings jwtSettings = builder.Configuration.GetSection("JwtConfig").Get<JwtConfigSettings>()
                ?? throw new InvalidOperationException("Missing JWT config.");

            OpenIDConfigSettings openIdSettings = builder.Configuration.GetSection("OpenIDConfig").Get<OpenIDConfigSettings>()
                ?? throw new InvalidOperationException("Missing Open ID Connect config.");

            string scheme = jwtSettings.AuthenticationScheme ?? JwtBearerDefaults.AuthenticationScheme;
            bool isDev = builder.Environment.IsDevelopment();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = scheme;
                options.DefaultChallengeScheme = scheme;
            })
            .SetJwtToken(jwtSettings, isDev)
            .SetOpenIDConnect(openIdSettings, isDev);

            return builder;
        }

        public WebApplicationBuilder SetFallbackPolicy()
        {
            AuthorizationPolicy requireAuthPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            builder.Services.AddAuthorizationBuilder()
                .SetFallbackPolicy(requireAuthPolicy);

            return builder;
        }

        public WebApplicationBuilder ConfigureRouting()
        {
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            return builder;
        }
    }
}
