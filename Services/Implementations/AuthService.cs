using Microsoft.AspNetCore.Identity;
using Star_Wars.DTOs;
using Star_Wars.Models;
using Star_Wars.Services.Interfaces;
using System.Security.Cryptography;

namespace Star_Wars.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse?> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return null;

            return await CreateAuthResponse(user);
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterDto registerDto)
        {
            if (await UserExistsAsync(registerDto.Email))
                return null;

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                return null;

            return await CreateAuthResponse(user);
        }

        private async Task<AuthResponse> CreateAuthResponse(ApplicationUser user)
        {
            var apiKey = GenerateApiKey();
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            user.ApiKey = apiKey;
            user.ApiKeyExpiresAt = expiresAt;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                Token = apiKey,
                Email = user.Email!,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                ExpiresAt = expiresAt
            };
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        private string GenerateApiKey()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("/", "_").Replace("+", "-").TrimEnd('=');
        }
    }
}
