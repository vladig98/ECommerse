namespace ECommerce.Catalog.Extensions;

internal static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        public WebApplication ConfigureScalar()
        {
            if (!app.Environment.IsDevelopment())
            {
                return app;
            }

            app.MapOpenApi().AllowAnonymous();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("ECommerce Catalog API")
                    .WithTheme(ScalarTheme.Purple)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            }).AllowAnonymous();

            string? urls = app.Configuration["ASPNETCORE_URLS"] ?? app.Configuration["urls"];
            string? targetUrl = urls?.Split(';').FirstOrDefault(u => u.StartsWith("https", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                return app;
            }

            ProcessStartInfo sclalarUIStartProcess = new($"{targetUrl}/scalar") { UseShellExecute = true };
            Process.Start(sclalarUIStartProcess);

            return app;
        }

        public WebApplication MapIdentityEndpoints()
        {
            app.MapGroup("/api/catalog")
                .MapProductEndpoints()
                .MapCategoryEndpoints()
                .MapAttributeEndpoints();

            return app;
        }

        public WebApplication ConfigureMiddlewares()
        {
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.ConfigureSerilogMiddleware();

            return app;
        }

        private WebApplication ConfigureSerilogMiddleware()
        {
            app.UseSerilogRequestLogging();

            return app;
        }

        public async Task<WebApplication> InitializeDatabase()
        {
            AsyncServiceScope scope = app.Services.CreateAsyncScope();
            await using ConfiguredAsyncDisposable _ = scope.ConfigureAwait(true);
            MainDbContext dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
            await dbContext.Database.MigrateAsync().ConfigureAwait(true);

            return app;
        }
    }
}
