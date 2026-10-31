using GlowCare.API.Enums;

namespace GlowCare.API.DTOs.Seller;

/// <summary>A seller application as seen by the Admin (includes applicant account info).
/// Named ...ListDto to avoid clashing with the request DTO in DTOs/Users.</summary>
public class SellerApplicationListDto
{
    public int SellerProfileId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string StoreName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Logo { get; set; } = string.Empty;

    public ApprovalStatus ApprovalStatus { get; set; }

    public DateTime CreatedAt { get; set; }
}
