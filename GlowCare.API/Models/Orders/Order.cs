using GlowCare.API.Enums;
using GlowCare.API.Models.Identity;
using GlowCare.API.Models.Promotions;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Orders;

/// <summary>
/// A customer order. Money fields are always calculated on the backend.
/// The shipping address is snapshotted as text so historical orders are not
/// affected by later address changes.
/// </summary>
public class Order
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>Subtotal + ShippingCost + TaxAmount - DiscountAmount.</summary>
    public decimal TotalAmount { get; set; }

    public decimal Subtotal { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    /// <summary>Snapshot of the chosen address at checkout time.</summary>
    [Required]
    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>Mobile phone number for delivery contact.</summary>
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>Set when a promo code was applied. FK to PromoCodes (Restrict).</summary>
    public int? PromoCodeId { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;

    public PromoCode? PromoCode { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
