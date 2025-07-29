using Microsoft.AspNetCore.Authorization;
using Star_Wars.Models;
using Star_Wars.Services.Interfaces;
using System.Security.Claims;

namespace Star_Wars.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
        {
            // Skip API key validation for auth endpoints and static content
            if (ShouldSkipApiKeyValidation(context))
            {
                await _next(context);
                return;
            }

            // Check if endpoint requires authorization
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<AuthorizeAttribute>() == null)
            {
                await _next(context);
                return;
            }

            // Get API key from header or cookie
            var apiKey = GetApiKeyFromRequest(context);
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key not found");
                await RespondWithUnauthorized(context, "API key required");
                return;
            }

            // Validate API key
            var user = await apiKeyService.ValidateApiKeyAsync(apiKey);
            if (user == null)
            {
                _logger.LogWarning("Invalid API key provided");
                await RespondWithUnauthorized(context, "Invalid API key");
                return;
            }

            // Set user context
            SetUserContext(context, user);
            
            _logger.LogDebug("API key validation successful for user: {Email}", user.Email);
            await _next(context);
        }

        private static string? GetApiKeyFromRequest(HttpContext context)
        {
            // Try header first, then cookie
            if (context.Request.Headers.TryGetValue("X-API-Key", out var headerValue))
                return headerValue;
            
            context.Request.Cookies.TryGetValue("StarWarsApiKey", out var cookieValue);
            return cookieValue;
        }

        private static void SetUserContext(HttpContext context, ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim("LastName", user.LastName ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, "ApiKey");
            context.User = new ClaimsPrincipal(identity);
        }

        private static async Task RespondWithUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(message);
        }

        private static bool ShouldSkipApiKeyValidation(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            
            // Skip for Swagger, auth endpoints, and static content
            return path?.StartsWith("/swagger") == true ||
                   path?.StartsWith("/health") == true ||
                   path?.StartsWith("/api/auth") == true ||
                   path == "/" ||
                   path == "/demo.html";
        }
    }

    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}