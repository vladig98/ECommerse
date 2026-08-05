Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder = builder.AddDatabase()
        .ConfigureRouting()
        .ConfigureSerilog()
        .ConfigureAuth()
        .MapSettings()
        .AddCache()
        .RegisterApplicationServices();

    WebApplication app = builder.Build();

    app = app.ConfigureMiddlewares()
        .ConfigureScalar()
        .MapIdentityEndpoints();

    app = await app.InitializeDatabase().ConfigureAwait(true);
    await app.RunAsync().ConfigureAwait(true);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(true);
}
