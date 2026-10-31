using GlowCare.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Promotions;

/// <summary>A promo code as returned to the Admin.</summary>
public class PromoCodeDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal MinimumOrderAmount { get; set; }

    public DateTime ExpiryDate { get; set; }

    public int UsageLimit { get; set; }

    public int TimesUsed { get; set; }

    public bool IsActive { get; set; }
}
