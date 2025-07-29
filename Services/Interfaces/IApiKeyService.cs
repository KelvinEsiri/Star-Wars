using Star_Wars.Models;

namespace Star_Wars.Services.Interfaces
{
    public interface IApiKeyService
    {
        Task<ApplicationUser?> ValidateApiKeyAsync(string apiKey);
        Task<bool> IsApiKeyValidAsync(string apiKey);
        Task<string> RegenerateApiKeyAsync(string userId);
        Task<bool> RevokeApiKeyAsync(string userId);
    }
}
