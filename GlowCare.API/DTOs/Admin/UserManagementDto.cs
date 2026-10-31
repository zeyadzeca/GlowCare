namespace GlowCare.API.DTOs.Admin;

/// <summary>A user account with their roles, for the admin user management screen.</summary>
public class UserManagementDto
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();

    /// <summary>True while the account is locked out after failed logins.</summary>
    public bool IsLockedOut { get; set; }
}
