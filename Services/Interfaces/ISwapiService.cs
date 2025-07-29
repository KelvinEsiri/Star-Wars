using Star_Wars.Models.SWAPI;

namespace Star_Wars.Services.Interfaces
{
    public interface ISwapiService
    {
        Task<List<SwapiStarship>> FetchAllStarshipsAsync();
        Task SeedStarshipsAsync();
    }
}
