using GlowCare.API.Enums;
using GlowCare.API.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.User;

/// <summary>
/// A seller's store profile. Created when a user applies to become a seller.
/// A user may have zero or one SellerProfile. Only Approved sellers can publish products.
/// </summary>
public class SellerProfile
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string StoreName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Logo { get; set; } = string.Empty;

    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
