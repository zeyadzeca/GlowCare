using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Promotions;

/// <summary>Request to check whether a promo code is valid for a given subtotal.</summary>
public class ValidatePromoCodeDto
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>The cart subtotal the code would apply to (used for the minimum-amount rule).</summary>
    [Range(0, double.MaxValue)]
    public decimal Subtotal { get; set; }
}
