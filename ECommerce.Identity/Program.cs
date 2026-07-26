WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder = builder.AddDatabase()
    .SetUpIdentity()
    .MapConfigurationSettings()
    .SetupAuthentication()
    .SetFallbackPolicy()
    .ConfigureRouting();

WebApplication app = builder.Build();

app = app.ConfigureScalar()
    .ConfigureMiddlewares()
    .MapIdentityEndpoints();

app = await app.InitializeDatabase();
await app.RunAsync();