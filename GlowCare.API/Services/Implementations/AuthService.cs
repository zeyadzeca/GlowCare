using GlowCare.API.DTOs.Auth;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Identity;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Handles customer registration, login, password reset and JWT token generation.
/// Passwords are hashed by Identity (never stored or logged in plain text).
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = true // no email confirmation flow in this project
        };

        // Identity validates the password against the configured password rules
        // and hashes it before storing.
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            throw new AppException(
                string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        // Every new registration starts as a Customer.
        // Sellers must apply afterwards; admins get the role seeded/assigned by admin.
        await _userManager.AddToRoleAsync(user, "Customer");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Same generic message for "no such user" and "wrong password"
        // so attackers cannot discover which emails are registered.
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            throw new AppException("Invalid email or password.", 401);
        }

        // Identity lockout: after too many failed attempts the account is locked.
        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new AppException("Account is locked due to too many failed attempts. Try again later.", 423);
        }

        // Successful login resets the failed-attempt counter.
        await _userManager.ResetAccessFailedCountAsync(user);

        return await BuildAuthResponseAsync(user);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
            // Return success even if user doesn't exist (security best practice)
            // to prevent email enumeration attacks
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", dto.Email);
            return;
        }

        // Generate password reset token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // In production, this would send an email with the reset link
        // For this academic project, we log the token (NOT recommended for production)
        _logger.LogInformation(
            "Password reset token for {Email}: {Token}. In production, send this via email.",
            dto.Email,
            token);

        // For now, return success - the frontend can show a message to check email
        // In a real implementation, you would integrate with an email service like SendGrid
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
            throw new AppException("Invalid email or reset token.", 400);
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

        if (!result.Succeeded)
        {
            throw new AppException(
                string.Join(" ", result.Errors.Select(e => e.Description)),
                400);
        }

        _logger.LogInformation("Password reset successful for {Email}", dto.Email);
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        return new AuthResponseDto
        {
            Token = GenerateJwtToken(user, roles, expiresAtUtc),
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Roles = roles.ToList()
        };
    }

    /// <summary>
    /// Creates the signed JWT. The token carries the user id, email, full name
    /// and role(s) - the same claims the rest of the API relies on for
    /// authorization (e.g. [Authorize(Roles = "Seller")]).
    /// </summary>
    private string GenerateJwtToken(ApplicationUser user, IList<string> roles, DateTime expiresAtUtc)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),   // our services read the user id from here
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // unique token id
        };

        // One claim per role, so role-based authorization works.
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
