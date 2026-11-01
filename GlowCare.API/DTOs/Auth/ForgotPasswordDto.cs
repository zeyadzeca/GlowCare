using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Auth;

/// <summary>Request to initiate password reset (user provides email).</summary>
public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
