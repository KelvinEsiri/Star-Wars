using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using AutoMapper;
using Star_Wars.Configuration;
using Star_Wars.Data;
using Star_Wars.Models;
using Star_Wars.Repositories.Implementations;
using Star_Wars.Repositories.Interfaces;
using Star_Wars.Services.Implementations;
using Star_Wars.Services.Interfaces;
using Star_Wars.Middleware;
using Star_Wars.Extensions;
using FluentValidation.AspNetCore;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Add Controllers and API configuration
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // Configure options from appsettings
    builder.Services.Configure<DatabaseSeedingOptions>(
        builder.Configuration.GetSection(DatabaseSeedingOptions.SectionName));
    
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Star Wars API", Version = "v1" });
        
        // Simple API key authentication for Swagger
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "API Key: X-API-Key",
            In = ParameterLocation.Header,
            Name = "X-API-Key",
            Type = SecuritySchemeType.ApiKey
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                },
                Array.Empty<string>()
            }
        });
    });

    // Register services
    builder.Services.AddHttpClient();
    builder.Services.AddAutoMapper(typeof(Program));
    builder.Services.AddFluentValidationAutoValidation();
    
    // Repository and service registration
    builder.Services.AddScoped<IStarshipRepository, StarshipRepository>();
    builder.Services.AddScoped<IStarshipService, StarshipService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
    builder.Services.AddScoped<ISwapiService, SwapiService>();
    builder.Services.AddScoped<IDatabaseSeederService, DatabaseSeederService>();

    var app = builder.Build();

    // Configure pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles(); // Enable static file serving from wwwroot
    app.UseMiddleware<ApiKeyMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Initialize and seed database on startup
    await app.InitializeDatabaseAsync();

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}

// Make Program class accessible for integration testing
public partial class Program { }
