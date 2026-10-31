namespace GlowCare.API.Helpers;

/// <summary>
/// Options for the "Admin" section of appsettings.json.
/// The initial admin account is created from secure configuration:
/// - Email and FullName: appsettings.json (non-secret)
/// - Password: User Secrets or environment variables ONLY (never committed)
/// </summary>
public class AdminSettings
{
    public const string SectionName = "Admin";

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>Read from User Secrets / environment variables only.</summary>
    public string Password { get; set; } = string.Empty;
}
