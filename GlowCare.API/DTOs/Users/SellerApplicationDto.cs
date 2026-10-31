using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Users;

/// <summary>
/// Request by a registered user to become a seller.
/// Creates a SellerProfile with ApprovalStatus = Pending; the Admin approves it.
/// </summary>
public class SellerApplicationDto
{
    [Required]
    [MaxLength(150)]
    public string StoreName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Logo { get; set; } = string.Empty;
}
