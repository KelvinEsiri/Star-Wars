namespace Star_Wars.Middleware
{
    public class AutoApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AutoApiKeyMiddleware> _logger;

        public AutoApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<AutoApiKeyMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only auto-inject for development environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment?.ToLower() == "development")
            {
                var apiKeyHeader = _configuration["ApiKeySettings:HeaderName"] ?? "X-API-Key";
                var apiKey = _configuration["ApiKeySettings:ApiKey"];

                // If request doesn't have API key, auto-inject it
                if (!context.Request.Headers.ContainsKey(apiKeyHeader) && !string.IsNullOrEmpty(apiKey))
                {
                    context.Request.Headers[apiKeyHeader] = apiKey;
                    _logger.LogDebug("Auto-injected API key for development request to {Path}", context.Request.Path);
                }
            }

            await _next(context);
        }
    }

    public static class AutoApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseAutoApiKey(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AutoApiKeyMiddleware>();
        }
    }
}
