using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Star_Wars.DTOs;
using Star_Wars.Services.Interfaces;

namespace Star_Wars.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Add back authorization - handled by middleware
    public class StarshipsController : ControllerBase
    {
        private readonly IStarshipService _starshipService;
        private readonly ILogger<StarshipsController> _logger;

        public StarshipsController(IStarshipService starshipService, ILogger<StarshipsController> logger)
        {
            _starshipService = starshipService;
            _logger = logger;
        }

        /// <summary>
        /// Get all starships with pagination, sorting, and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<StarshipDto>>> GetStarships([FromQuery] StarshipQueryDto query)
        {
            try
            {
                var result = await _starshipService.GetAllAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting starships");
                return StatusCode(500, "An error occurred while retrieving starships");
            }
        }

        /// <summary>
        /// Get a specific starship by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<StarshipDto>> GetStarship(int id)
        {
            try
            {
                var starship = await _starshipService.GetByIdAsync(id);
                if (starship == null)
                {
                    return NotFound($"Starship with ID {id} not found");
                }
                return Ok(starship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting starship with ID {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the starship");
            }
        }

        /// <summary>
        /// Create a new starship
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<StarshipDto>> CreateStarship([FromBody] CreateStarshipDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var starship = await _starshipService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetStarship), new { id = starship.Id }, starship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating starship");
                return StatusCode(500, "An error occurred while creating the starship");
            }
        }

        /// <summary>
        /// Update an existing starship
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<StarshipDto>> UpdateStarship(int id, [FromBody] UpdateStarshipDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var starship = await _starshipService.UpdateAsync(id, dto);
                if (starship == null)
                {
                    return NotFound($"Starship with ID {id} not found");
                }

                return Ok(starship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating starship with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the starship");
            }
        }

        /// <summary>
        /// Delete a starship (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStarship(int id)
        {
            try
            {
                var result = await _starshipService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Starship with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting starship with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the starship");
            }
        }

        /// <summary>
        /// Get all manufacturers
        /// </summary>
        [HttpGet("manufacturers")]
        public async Task<ActionResult<List<string>>> GetManufacturers()
        {
            try
            {
                var manufacturers = await _starshipService.GetManufacturersAsync();
                return Ok(manufacturers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting manufacturers");
                return StatusCode(500, "An error occurred while retrieving manufacturers");
            }
        }

        /// <summary>
        /// Get all starship classes
        /// </summary>
        [HttpGet("classes")]
        public async Task<ActionResult<List<string>>> GetStarshipClasses()
        {
            try
            {
                var classes = await _starshipService.GetStarshipClassesAsync();
                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting starship classes");
                return StatusCode(500, "An error occurred while retrieving starship classes");
            }
        }
    }
}
