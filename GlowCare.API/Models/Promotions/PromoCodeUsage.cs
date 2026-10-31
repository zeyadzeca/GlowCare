using GlowCare.API.Models.Identity;
using GlowCare.API.Models.Orders;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Promotions;

/// <summary>
/// Tracks one customer's use of a promo code on one order.
/// The unique index on (PromoCodeId, UserId) enforces "one use per customer
/// per code" at the database level.
/// </summary>
public class PromoCodeUsage
{
    public int Id { get; set; }

    public int PromoCodeId { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public int OrderId { get; set; }

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public PromoCode PromoCode { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    public Order Order { get; set; } = null!;
}
