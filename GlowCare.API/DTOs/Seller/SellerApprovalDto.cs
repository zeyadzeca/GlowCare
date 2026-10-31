using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Seller;

/// <summary>Request to approve or reject a seller application (Admin only).</summary>
public class SellerApprovalDto
{
    [Required]
    public bool IsApproved { get; set; }
}
