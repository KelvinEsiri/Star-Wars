using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Star_Wars.Data;
using Star_Wars.Models;
using System.Security.Claims;

namespace Star_Wars.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Order 66 - Execute complete database purge of all user data
        /// ⚠️ WARNING: This will permanently delete ALL users and data
        /// </summary>
        [HttpDelete("order-66")]
        public async Task<IActionResult> Order66([FromBody] Order66Request request)
        {
            try
            {
                // Validate user and admin key
                if (!ValidateOrder66Request(request))
                {
                    return Unauthorized("Invalid authorization");
                }

                var currentUser = await GetCurrentUser();
                
                // Log the operation
                _logger.LogCritical("🔴 ORDER 66 INITIATED by {Email} at {Timestamp}", 
                    currentUser?.Email, DateTime.UtcNow);

                // Execute database purge
                var result = await ExecuteDatabasePurge();
                
                _logger.LogCritical("🔴 ORDER 66 COMPLETED: {UsersDeleted} users, {StarshipsDeleted} starships deleted", 
                    result.UsersDeleted, result.StarshipsDeleted);

                return Ok(new
                {
                    message = "Order 66 executed successfully. The galaxy has been purged.",
                    timestamp = DateTime.UtcNow,
                    deletedCounts = result,
                    executedBy = currentUser?.Email,
                    reason = request.Reason ?? "No reason provided"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order 66 execution failed");
                return StatusCode(500, "Order 66 execution failed");
            }
        }

        private bool ValidateOrder66Request(Order66Request request)
        {
            var adminKey = _configuration["AdminSettings:Order66Key"];
            
            return !string.IsNullOrEmpty(adminKey) && 
                   request.AdminKey == adminKey && 
                   request.ConfirmationPhrase == "Execute Order 66";
        }

        private async Task<ApplicationUser?> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId) ? null : await _userManager.FindByIdAsync(userId);
        }

        private async Task<(int UsersDeleted, int StarshipsDeleted)> ExecuteDatabasePurge()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            var userCount = await _context.Users.CountAsync();
            var starshipCount = await _context.Starships.CountAsync();

            // Delete all data
            _context.Starships.RemoveRange(_context.Starships);
            _context.Users.RemoveRange(_context.Users);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (userCount, starshipCount);
        }

        /// <summary>
        /// Get Order 66 status and requirements
        /// </summary>
        [HttpGet("order-66/info")]
        public IActionResult GetOrder66Info()
        {
            return Ok(new
            {
                Title = "Order 66 - Database Purge Operation",
                Description = "Execute Order 66 to eliminate all users and their data",
                Warning = "⚠️ This operation is IRREVERSIBLE",
                Requirements = new
                {
                    Authentication = "Valid API key required",
                    AdminKey = "Admin authorization key required",
                    ConfirmationPhrase = "Exact phrase: 'Execute Order 66'"
                }
            });
        }
    }

    /// <summary>
    /// Request model for Order 66 execution
    /// </summary>
    public class Order66Request
    {
        /// <summary>
        /// Admin authorization key required to execute Order 66
        /// This should be configured in appsettings.json under AdminSettings:Order66Key
        /// </summary>
        public string AdminKey { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation phrase that must be exactly "Execute Order 66"
        /// This prevents accidental execution
        /// </summary>
        public string ConfirmationPhrase { get; set; } = string.Empty;

        /// <summary>
        /// Optional reason for executing Order 66 (logged for audit purposes)
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Response model for successful Order 66 execution
    /// </summary>
    public class Order66Response
    {
        public string Message { get; set; } = string.Empty;
        public string ExecutedBy { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
        public int UsersDeleted { get; set; }
        public int StarshipsDeleted { get; set; }
        public string Warning { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
