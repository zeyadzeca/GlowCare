using GlowCare.API.Enums;

namespace GlowCare.API.DTOs.Promotions;

/// <summary>Result of validating a promo code during checkout.</summary>
public class PromoCodeValidationDto
{
    public bool IsValid { get; set; }

    /// <summary>Human-readable reason when invalid (or confirmation when valid).</summary>
    public string Message { get; set; } = string.Empty;

    public int? PromoCodeId { get; set; }

    public DiscountType? DiscountType { get; set; }

    public decimal? DiscountValue { get; set; }

    /// <summary>The money amount this code would subtract from the subtotal.</summary>
    public decimal DiscountAmount { get; set; }
}
