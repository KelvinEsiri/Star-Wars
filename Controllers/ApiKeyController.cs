using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Star_Wars.Services.Interfaces;
using System.Security.Claims;

namespace Star_Wars.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ILogger<ApiKeyController> _logger;

        public ApiKeyController(IApiKeyService apiKeyService, ILogger<ApiKeyController> logger)
        {
            _apiKeyService = apiKeyService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's API key information
        /// </summary>
        [HttpGet("info")]
        public ActionResult GetApiKeyInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            
            return Ok(new
            {
                message = "API key is active and valid",
                userId = userId,
                email = email,
                headerName = "X-API-Key",
                instructions = "Use your API key in the X-API-Key header for all protected endpoints"
            });
        }

        /// <summary>
        /// Regenerate the current user's API key
        /// </summary>
        [HttpPost("regenerate")]
        public async Task<ActionResult> RegenerateApiKey()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found");
                }

                var newApiKey = await _apiKeyService.RegenerateApiKeyAsync(userId);

                // Automatically set the new API key as a cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false, // Allow JavaScript access for easier debugging
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(30) // Same expiration as the API key
                };

                Response.Cookies.Append("StarWarsApiKey", newApiKey, cookieOptions);
                
                _logger.LogInformation("New API key cookie set for user {UserId}", userId);
                
                return Ok(new
                {
                    message = "API key regenerated successfully",
                    newApiKey = newApiKey,
                    headerName = "X-API-Key",
                    expiresAt = DateTime.UtcNow.AddMinutes(30),
                    warning = "Your previous API key has been revoked. Update your applications with the new key.",
                    note = "New API key has been automatically saved as a cookie for browser access"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating API key");
                return StatusCode(500, "Failed to regenerate API key");
            }
        }

        /// <summary>
        /// Revoke the current user's API key (logout)
        /// </summary>
        [HttpPost("revoke")]
        public async Task<ActionResult> RevokeApiKey()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found");
                }

                var result = await _apiKeyService.RevokeApiKeyAsync(userId);
                if (!result)
                {
                    return BadRequest("Failed to revoke API key");
                }

                // Clear the API key cookie
                Response.Cookies.Delete("StarWarsApiKey");
                _logger.LogInformation("API key cookie cleared for user {UserId}", userId);

                return Ok(new
                {
                    message = "API key revoked successfully",
                    note = "You will need to login again to get a new API key. Cookie has been cleared."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key");
                return StatusCode(500, "Failed to revoke API key");
            }
        }
    }
}
