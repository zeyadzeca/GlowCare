using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Admin;

/// <summary>Request to update a user's profile fields (Admin only).</summary>
public class UpdateUserDto
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    public bool IsLockedOut { get; set; }
}
