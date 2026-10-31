using GlowCare.API.DTOs.Auth;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// Registration, login and JWT token generation.
/// Password hashing is handled by ASP.NET Core Identity.
/// </summary>
public interface IAuthService
{
    /// <summary>Registers a new customer account and returns a JWT.</summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    /// <summary>Logs in with email/password and returns a JWT.</summary>
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
