using Microsoft.EntityFrameworkCore;
using ShoppingCartService.EventHandlers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration[GlobalConstants.ConnectionString] ?? throw new InvalidOperationException(GlobalConstants.InvalidConnectionString);

builder.Services.AddDbContext<ShoppingCartDbContext>(options =>
{
    options.UseNpgsql(connectionString,
    npgsqlOptionsAction: npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);

        //Configuring Connection Resiliency:
        npgsqlOptions.
            EnableRetryOnFailure(maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});

builder.Services.AddHostedService<EventBusSubscriber>();
builder.Services.AddTransient<ICartService, CartService>();
builder.Services.AddTransient<ICartRepository, CartRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
