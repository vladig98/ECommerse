using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using UserManagementService.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:PostgreSQL"] ??
        throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<ECommerceDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddIdentityCore<User>().AddSignInManager().AddRoles<Role>().AddEntityFrameworkStores<ECommerceDbContext>().AddDefaultTokenProviders();

builder.Services.AddDataProtection();
builder.Services.AddSingleton<System.TimeProvider>(System.TimeProvider.System);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
