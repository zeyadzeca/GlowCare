using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Users;

/// <summary>Request to update the profile of the authenticated user.</summary>
public class UpdateProfileDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
