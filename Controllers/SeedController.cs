using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Star_Wars.Services.Interfaces;

namespace Star_Wars.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Add back authorization - handled by middleware
    public class SeedController : ControllerBase
    {
        private readonly ISwapiService _swapiService;
        private readonly ILogger<SeedController> _logger;

        public SeedController(ISwapiService swapiService, ILogger<SeedController> logger)
        {
            _swapiService = swapiService;
            _logger = logger;
        }

        /// <summary>
        /// Seed starships from SWAPI
        /// </summary>
        [HttpPost("starships")]
        public async Task<ActionResult> SeedStarships()
        {
            try
            {
                await _swapiService.SeedStarshipsAsync();
                return Ok("Starships seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding starships");
                return StatusCode(500, "An error occurred while seeding starships");
            }
        }
    }
}
