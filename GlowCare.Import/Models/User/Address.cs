using GlowCare.Import.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.Import.Models.User;

/// <summary>
/// A shipping address belonging to a registered user.
/// One user can have many addresses. No IsDefault by design.
/// </summary>
public class Address
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string BuildingNumber { get; set; } = string.Empty;

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
