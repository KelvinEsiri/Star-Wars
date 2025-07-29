using AutoMapper;
using Star_Wars.DTOs;
using Star_Wars.Models;
using Star_Wars.Repositories.Interfaces;
using Star_Wars.Services.Interfaces;

namespace Star_Wars.Services.Implementations
{
    public class StarshipService : IStarshipService
    {
        private readonly IStarshipRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<StarshipService> _logger;

        public StarshipService(
            IStarshipRepository repository,
            IMapper mapper,
            ILogger<StarshipService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<StarshipDto>> GetAllAsync(StarshipQueryDto query)
        {
            try
            {
                var result = await _repository.GetAllAsync(query);
                return new PagedResult<StarshipDto>
                {
                    Data = _mapper.Map<List<StarshipDto>>(result.Data),
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages,
                    HasNext = result.HasNext,
                    HasPrevious = result.HasPrevious
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting starships with query {@Query}", query);
                throw;
            }
        }

        public async Task<StarshipDto?> GetByIdAsync(int id)
        {
            try
            {
                var starship = await _repository.GetByIdAsync(id);
                return starship != null ? _mapper.Map<StarshipDto>(starship) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting starship with id {Id}", id);
                throw;
            }
        }

        public async Task<StarshipDto> CreateAsync(CreateStarshipDto dto)
        {
            try
            {
                var starship = _mapper.Map<Starship>(dto);
                var createdStarship = await _repository.CreateAsync(starship);
                _logger.LogInformation("Created starship {Name} with id {Id}", createdStarship.Name, createdStarship.Id);
                return _mapper.Map<StarshipDto>(createdStarship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating starship {@Dto}", dto);
                throw;
            }
        }

        public async Task<StarshipDto?> UpdateAsync(int id, UpdateStarshipDto dto)
        {
            try
            {
                var existingStarship = await _repository.GetByIdAsync(id);
                if (existingStarship == null)
                    return null;

                _mapper.Map(dto, existingStarship);
                var updatedStarship = await _repository.UpdateAsync(existingStarship);
                _logger.LogInformation("Updated starship {Name} with id {Id}", updatedStarship.Name, updatedStarship.Id);
                return _mapper.Map<StarshipDto>(updatedStarship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating starship with id {Id} and dto {@Dto}", id, dto);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var result = await _repository.DeleteAsync(id);
                if (result)
                {
                    _logger.LogInformation("Deleted starship with id {Id}", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting starship with id {Id}", id);
                throw;
            }
        }

        public async Task<List<string>> GetManufacturersAsync()
        {
            try
            {
                return await _repository.GetManufacturersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manufacturers");
                throw;
            }
        }

        public async Task<List<string>> GetStarshipClassesAsync()
        {
            try
            {
                return await _repository.GetStarshipClassesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting starship classes");
                throw;
            }
        }
    }
}
