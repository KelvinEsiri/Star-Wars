using Star_Wars.DTOs;
using Star_Wars.Models;

namespace Star_Wars.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginDto loginDto);
        Task<AuthResponse?> RegisterAsync(RegisterDto registerDto);
        Task<bool> UserExistsAsync(string email);
    }
}
