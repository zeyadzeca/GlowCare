namespace GlowCare.API.DTOs.Auth;

/// <summary>
/// Returned after a successful registration or login.
/// The frontend stores the Token and sends it in the Authorization header.
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public IList<string> Roles { get; set; } = new List<string>();
}
