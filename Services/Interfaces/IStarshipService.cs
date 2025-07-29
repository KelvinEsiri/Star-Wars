using Star_Wars.DTOs;
using Star_Wars.Models;

namespace Star_Wars.Services.Interfaces
{
    public interface IStarshipService
    {
        Task<PagedResult<StarshipDto>> GetAllAsync(StarshipQueryDto query);
        Task<StarshipDto?> GetByIdAsync(int id);
        Task<StarshipDto> CreateAsync(CreateStarshipDto dto);
        Task<StarshipDto?> UpdateAsync(int id, UpdateStarshipDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<string>> GetManufacturersAsync();
        Task<List<string>> GetStarshipClassesAsync();
    }
}
