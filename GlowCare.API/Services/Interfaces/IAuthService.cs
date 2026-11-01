using GlowCare.API.DTOs.Auth;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// Registration, login, password reset and JWT token generation.
/// Password hashing is handled by ASP.NET Core Identity.
/// </summary>
public interface IAuthService
{
    /// <summary>Registers a new customer account and returns a JWT.</summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    /// <summary>Logs in with email/password and returns a JWT.</summary>
    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    /// <summary>Initiates password reset by generating a reset token for the email.</summary>
    Task ForgotPasswordAsync(ForgotPasswordDto dto);

    /// <summary>Resets password using the token from ForgotPassword.</summary>
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
