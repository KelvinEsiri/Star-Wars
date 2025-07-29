using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Star_Wars.Models;
using Star_Wars.Models.SWAPI;
using Star_Wars.Repositories.Interfaces;
using Star_Wars.Services.Interfaces;
using System.Linq;

namespace Star_Wars.Services.Implementations
{
    public class SwapiService : ISwapiService
    {
        private readonly HttpClient _httpClient;
        private readonly IStarshipRepository _starshipRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SwapiService> _logger;
        private readonly string _swapiBaseUrl;

        public SwapiService(
            HttpClient httpClient,
            IStarshipRepository starshipRepository,
            IMapper mapper,
            ILogger<SwapiService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _starshipRepository = starshipRepository;
            _mapper = mapper;
            _logger = logger;
            _swapiBaseUrl = configuration["SwapiSettings:BaseUrl"] ?? "https://swapi.info/api/starships";
        }

        public async Task<List<SwapiStarship>> FetchAllStarshipsAsync()
        {
            var allStarships = new List<SwapiStarship>();

            try
            {
                _logger.LogInformation("Fetching starships from: {Url}", _swapiBaseUrl);
                
                var response = await _httpClient.GetAsync(_swapiBaseUrl);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                
                // Deserialize as direct array (swapi.info returns all starships in one response)
                var starshipsArray = JsonConvert.DeserializeObject<List<SwapiStarship>>(jsonContent);
                if (starshipsArray != null && starshipsArray.Any())
                {
                    allStarships.AddRange(starshipsArray);
                    _logger.LogInformation("Fetched {Count} starships from direct array response", starshipsArray.Count);
                }
                else
                {
                    _logger.LogWarning("No starships found in response");
                }

                _logger.LogInformation("Successfully fetched {TotalCount} starships from SWAPI", allStarships.Count);
                return allStarships;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching starships from SWAPI");
                throw;
            }
        }

        public async Task SeedStarshipsAsync()
        {
            try
            {
                _logger.LogInformation("Starting starship seeding process");

                var swapiStarships = await FetchAllStarshipsAsync();
                var newStarships = new List<Starship>();

                foreach (var swapiStarship in swapiStarships)
                {
                    // Check if starship already exists
                    if (!await _starshipRepository.ExistsAsync(swapiStarship.Name))
                    {
                        var starship = _mapper.Map<Starship>(swapiStarship);
                        newStarships.Add(starship);
                    }
                }

                if (newStarships.Any())
                {
                    await _starshipRepository.BulkInsertAsync(newStarships);
                    _logger.LogInformation("Successfully seeded {Count} new starships", newStarships.Count);
                }
                else
                {
                    _logger.LogInformation("No new starships to seed - all starships already exist in database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding starships");
                throw;
            }
        }
    }
}
