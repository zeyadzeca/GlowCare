namespace GlowCare.API.DTOs.Users;

/// <summary>The authenticated user's profile as returned by the API.</summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();
}
