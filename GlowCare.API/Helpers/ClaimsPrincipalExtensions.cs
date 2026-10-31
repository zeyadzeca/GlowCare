using System.Security.Claims;

namespace GlowCare.API.Helpers;

/// <summary>
/// Small helper so controllers read the caller's identity from JWT claims
/// instead of trusting anything in the request body/route.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>The ApplicationUser Id of the authenticated caller (null for anonymous).</summary>
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);
}
