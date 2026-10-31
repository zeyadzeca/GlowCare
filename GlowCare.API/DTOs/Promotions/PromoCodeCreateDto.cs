using GlowCare.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Promotions;

/// <summary>Request to create a promo code (Admin only).</summary>
public class PromoCodeCreateDto
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public DiscountType DiscountType { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be positive.")]
    public decimal DiscountValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumOrderAmount { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Range(1, int.MaxValue)]
    public int UsageLimit { get; set; }
}
