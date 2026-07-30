Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder = builder.AddDatabase()
        .ConfigureRouting()
        .ConfigureSerilog()
        .ConfigureAuth()
        .MapSettings()
        .RegisterApplicationServices();

    WebApplication app = builder.Build();

    app = app.ConfigureMiddlewares()
        .ConfigureScalar()
        .MapIdentityEndpoints();

    app = await app.InitializeDatabase();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
