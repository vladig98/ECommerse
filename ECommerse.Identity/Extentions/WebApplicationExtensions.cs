namespace ECommerse.Identity.Extentions;

public static class WebApplicationExtensions
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
                options.WithTitle("ECommerse Identity API")
                    .WithTheme(ScalarTheme.Purple)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            }).AllowAnonymous();

            ProcessStartInfo sclalarUIStartProcess = new("https://localhost:7031/scalar") { UseShellExecute = true };
            Process.Start(sclalarUIStartProcess);

            return app;
        }

        public WebApplication MapIdentityEndpoints()
        {
            app.MapGroup("/api/identity")
               .MapIdentityApi<IdentityUser>()
               .AllowAnonymous();

            return app;
        }

        public WebApplication ConfigureMiddlewares()
        {
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }

        public async Task<WebApplication> InitializeDatabase()
        {
            await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
            MainDbContext dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
            await dbContext.Database.MigrateAsync();

            return app;
        }
    }
}
