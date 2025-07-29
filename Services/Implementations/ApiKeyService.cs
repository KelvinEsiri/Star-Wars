using Microsoft.AspNetCore.Identity;
using Star_Wars.Models;
using Star_Wars.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Star_Wars.Services.Implementations
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ApiKeyService> _logger;

        public ApiKeyService(UserManager<ApplicationUser> userManager, ILogger<ApiKeyService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ApplicationUser?> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey))
                    return null;

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.ApiKey == apiKey && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Invalid API key attempted: {ApiKey}", apiKey.Substring(0, Math.Min(8, apiKey.Length)) + "...");
                    return null;
                }

                // Check if API key has expired
                if (user.ApiKeyExpiresAt.HasValue && user.ApiKeyExpiresAt.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("Expired API key used for user: {Email}", user.Email);
                    return null;
                }

                _logger.LogDebug("Valid API key used for user: {Email}", user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key");
                return null;
            }
        }

        public async Task<bool> IsApiKeyValidAsync(string apiKey)
        {
            var user = await ValidateApiKeyAsync(apiKey);
            return user != null;
        }

        public async Task<string> RegenerateApiKeyAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempt to regenerate API key for non-existent user: {UserId}", userId);
                    throw new ArgumentException("User not found");
                }

                var newApiKey = GenerateApiKey();
                var expiresAt = DateTime.UtcNow.AddMinutes(30);

                user.ApiKey = newApiKey;
                user.ApiKeyExpiresAt = expiresAt;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to regenerate API key for user: {UserId}", userId);
                    throw new InvalidOperationException("Failed to update user API key");
                }

                _logger.LogInformation("API key regenerated for user: {Email}", user.Email);
                return newApiKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating API key for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RevokeApiKeyAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempt to revoke API key for non-existent user: {UserId}", userId);
                    return false;
                }

                user.ApiKey = null;
                user.ApiKeyExpiresAt = null;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to revoke API key for user: {UserId}", userId);
                    return false;
                }

                _logger.LogInformation("API key revoked for user: {Email}", user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key for user: {UserId}", userId);
                return false;
            }
        }

        private string GenerateApiKey()
        {
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }
    }
}
