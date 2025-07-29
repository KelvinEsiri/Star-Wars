using Star_Wars.DTOs;
using Star_Wars.Models;

namespace Star_Wars.Repositories.Interfaces
{
    public interface IStarshipRepository
    {
        Task<PagedResult<Starship>> GetAllAsync(StarshipQueryDto query);
        Task<Starship?> GetByIdAsync(int id);
        Task<Starship> CreateAsync(Starship starship);
        Task<Starship> UpdateAsync(Starship starship);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string name);
        Task<int> BulkInsertAsync(List<Starship> starships);
        Task<List<string>> GetManufacturersAsync();
        Task<List<string>> GetStarshipClassesAsync();
    }
}
