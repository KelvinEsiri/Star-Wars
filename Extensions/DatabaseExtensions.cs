using Microsoft.Extensions.Options;
using Star_Wars.Configuration;
using Star_Wars.Services.Interfaces;

namespace Star_Wars.Extensions
{
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Initialize and seed the database on application startup
        /// </summary>
        /// <param name="app">The web application</param>
        /// <returns>The web application for chaining</returns>
        public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var seederService = scope.ServiceProvider.GetRequiredService<IDatabaseSeederService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var seedingOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseSeedingOptions>>().Value;

            if (!seedingOptions.SeedOnStartup)
            {
                logger.LogInformation("Database seeding on startup is disabled in configuration");
                return app;
            }

            try
            {
                logger.LogInformation("Starting database initialization...");
                await seederService.SeedDatabaseAsync();
                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database initialization failed. Application will continue to start");
                // Don't throw - allow app to start even if seeding fails
            }

            return app;
        }
    }
}
