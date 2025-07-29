using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Star_Wars.Configuration;
using Star_Wars.Data;
using Star_Wars.Services.Interfaces;

namespace Star_Wars.Services.Implementations
{
    public class DatabaseSeederService : IDatabaseSeederService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISwapiService _swapiService;
        private readonly ILogger<DatabaseSeederService> _logger;
        private readonly DatabaseSeedingOptions _seedingOptions;

        public DatabaseSeederService(
            ApplicationDbContext context,
            ISwapiService swapiService,
            ILogger<DatabaseSeederService> logger,
            IOptions<DatabaseSeedingOptions> seedingOptions)
        {
            _context = context;
            _swapiService = swapiService;
            _logger = logger;
            _seedingOptions = seedingOptions.Value;
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            try
            {
                _logger.LogInformation("Ensuring database is created...");
                
                // Apply any pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Database migrations applied successfully");
                }
                else
                {
                    _logger.LogInformation("Database is up to date - no pending migrations");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring database is created");
                throw;
            }
        }

        public async Task SeedDatabaseAsync()
        {
            try
            {
                if (!_seedingOptions.EnableAutoSeed)
                {
                    _logger.LogInformation("Automatic database seeding is disabled in configuration");
                    return;
                }

                _logger.LogInformation("Starting automatic database seeding...");

                // Ensure database is created and up to date first
                await EnsureDatabaseCreatedAsync();

                // Check if we have any starships in the database
                var starshipCount = await _context.Starships.CountAsync();
                
                if (starshipCount == 0 || _seedingOptions.ForceReseed)
                {
                    if (_seedingOptions.ForceReseed && starshipCount > 0)
                    {
                        _logger.LogInformation("Force reseed is enabled. Clearing existing starships and reseeding...");
                        // Optionally clear existing data if force reseed is enabled
                        _context.Starships.RemoveRange(_context.Starships);
                        await _context.SaveChangesAsync();
                    }

                    _logger.LogInformation("Starting starship seeding from SWAPI...");
                    await _swapiService.SeedStarshipsAsync();
                    _logger.LogInformation("Automatic starship seeding completed successfully");
                }
                else
                {
                    _logger.LogInformation("Database already contains {Count} starships. Skipping automatic seeding", starshipCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during automatic database seeding");
                // Don't rethrow - we don't want seeding errors to prevent app startup
                // The application should still start even if seeding fails
            }
        }
    }
}
