namespace GlowCare.API.Helpers;

/// <summary>
/// strongly typed options for the "Jwt" section of appsettings.json.
/// The signing key is NOT part of this class registration from appsettings -
/// it is supplied via User Secrets or environment variables (never committed).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; }

    /// <summary>Read from User Secrets / environment variables only.</summary>
    public string SecretKey { get; set; } = string.Empty;
}
