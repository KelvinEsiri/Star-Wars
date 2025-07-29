using Star_Wars.Data;

namespace Star_Wars.Services.Interfaces
{
    public interface IDatabaseSeederService
    {
        Task SeedDatabaseAsync();
        Task EnsureDatabaseCreatedAsync();
    }
}
