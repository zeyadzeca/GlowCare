using GlowCare.API.DTOs.Auth;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>Registration (starts as Customer) and login. Both return a JWT.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Registers a new customer account and returns the JWT immediately.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        => Ok(await _auth.RegisterAsync(dto));

    /// <summary>Logs in with email/password (generic error on bad credentials, lockout after 5 tries).</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        => Ok(await _auth.LoginAsync(dto));

    /// <summary>Initiates password reset - generates a token (in production, this would send an email).</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        await _auth.ForgotPasswordAsync(dto);
        return Ok(new { message = "If an account with this email exists, a password reset link has been sent." });
    }

    /// <summary>Resets password using the token from forgot-password.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        await _auth.ResetPasswordAsync(dto);
        return Ok(new { message = "Password has been reset successfully." });
    }
}
