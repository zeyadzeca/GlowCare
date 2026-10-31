using GlowCare.API.Enums;

namespace GlowCare.API.DTOs.Users;

/// <summary>The seller's own profile and approval status.</summary>
public class SellerProfileDto
{
    public int Id { get; set; }

    public string StoreName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Logo { get; set; } = string.Empty;

    public ApprovalStatus ApprovalStatus { get; set; }

    public DateTime CreatedAt { get; set; }
}
