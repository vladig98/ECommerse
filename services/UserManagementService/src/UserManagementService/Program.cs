using Confluent.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

#region Database
string connectionString = builder.Configuration["ConnectionStrings:PostgreSQL"] ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ECommerceDbContext>(options =>
{
    options.UseNpgsql(connectionString,
    npgsqlOptionsAction: npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);

        npgsqlOptions.
            EnableRetryOnFailure(maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});

builder.Services.AddIdentityCore<User>().AddSignInManager().AddRoles<Role>().AddEntityFrameworkStores<ECommerceDbContext>().AddDefaultTokenProviders();
#endregion

#region JWT_Configuration
string jwtIssuer = builder.Configuration["UserManagement:JWT:Issuer"] ?? throw new InvalidOperationException("Invalid JWT configuration");
string jwtKey = builder.Configuration["UserManagement:JWT:Key"] ?? throw new InvalidOperationException("Invalid JWT configuration");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.IncludeErrorDetails = true;
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtIssuer,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
         ClockSkew = TimeSpan.Zero
     };
 });
#endregion

#region Services_registration
builder.Services.AddAutoMapper(typeof(AutoMapperConfig));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IRegisterService, RegisterService>();
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IProfileService, ProfileService>();
builder.Services.AddTransient<IRoleManagement, RoleManagement>();
builder.Services.AddTransient<IDataFactory, DataFactory>();
builder.Services.AddTransient<IUserManagement, UserManagement>();
builder.Services.AddScoped(typeof(CancellationToken), cfg =>
{
    IHttpContextAccessor httpContext = cfg.GetRequiredService<IHttpContextAccessor>();
    return httpContext.HttpContext?.RequestAborted ?? CancellationToken.None;
});
builder.Services.AddSingleton(cfg =>
{
    ProducerConfig config = new ProducerConfig();
    config.BootstrapServers = "localhost:9092";

    return new ProducerBuilder<string, UserCreatedEvent>(config).SetValueSerializer(new KafkaValueSerializer<UserCreatedEvent>()).Build();
});
builder.Services.AddTransient<IKafkaEventProducer<string, UserCreatedEvent>, KafkaEventsProdcuer<string, UserCreatedEvent>>();
#endregion

#region ASP_Services
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
#endregion

#region App_Building
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
#endregion