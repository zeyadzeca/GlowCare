using GlowCare.API.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.User;

/// <summary>
/// A phone number belonging to a registered user.
/// Similar to Address - one user can have multiple phone numbers.
/// </summary>
public class UserPhoneNumber
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
