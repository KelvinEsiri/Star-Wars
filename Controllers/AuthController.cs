using Microsoft.AspNetCore.Mvc;
using Star_Wars.DTOs;
using Star_Wars.Services.Interfaces;

namespace Star_Wars.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _authService.LoginAsync(loginDto);
                if (result == null)
                {
                    return Unauthorized("Invalid email or password");
                }

                // Automatically set the generated API key as a cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false, // Allow JavaScript access for easier debugging
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = result.ExpiresAt // Use the same expiration as the token
                };

                Response.Cookies.Append("StarWarsApiKey", result.Token, cookieOptions);
                
                _logger.LogInformation("API key cookie set for user {Email}", result.Email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, "An error occurred during login");
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await _authService.UserExistsAsync(registerDto.Email))
                {
                    return BadRequest("User with this email already exists");
                }

                var result = await _authService.RegisterAsync(registerDto);
                if (result == null)
                {
                    return BadRequest("Registration failed");
                }

                // Automatically set the generated API key as a cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false, // Allow JavaScript access for easier debugging
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = result.ExpiresAt // Use the same expiration as the token
                };

                Response.Cookies.Append("StarWarsApiKey", result.Token, cookieOptions);
                
                _logger.LogInformation("API key cookie set for newly registered user {Email}", result.Email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, "An error occurred during registration");
            }
        }
    }
}
