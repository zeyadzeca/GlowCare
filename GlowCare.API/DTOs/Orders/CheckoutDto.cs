using GlowCare.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Orders;

/// <summary>
/// The checkout request. The backend recalculates ALL money values -
/// nothing price-related is trusted from the frontend.
/// </summary>
public class CheckoutDto
{
    /// <summary>Id of one of the customer's own addresses (snapshotted into the order).</summary>
    [Required]
    public int AddressId { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Optional promo code entered by the customer (validated server-side).</summary>
    [MaxLength(50)]
    public string? PromoCode { get; set; }

    /// <summary>Mobile phone number for delivery contact.</summary>
    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
}
