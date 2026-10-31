using GlowCare.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Promotions;

/// <summary>
/// A promotional code. All discount calculations happen on the backend.
/// Usage rules: expiry date, usage limit, minimum order amount, IsActive.
/// Deactivate instead of deleting so order history stays intact (FK Restrict).
/// </summary>
public class PromoCode
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    /// <summary>Percentage (e.g. 10) or fixed amount (e.g. 50 EGP), depending on DiscountType.</summary>
    public decimal DiscountValue { get; set; }

    /// <summary>Order subtotal must reach this amount before the code applies.</summary>
    public decimal MinimumOrderAmount { get; set; }

    public DateTime ExpiryDate { get; set; }

    /// <summary>Maximum number of times this code may be used across all customers.</summary>
    public int UsageLimit { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<PromoCodeUsage> Usages { get; set; } = new List<PromoCodeUsage>();
}
