using GlowCare.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Orders;

/// <summary>
/// A payment record for an order.
///
/// SECURITY: This table NEVER stores credit card numbers, CVV or any card
/// details - card data is handled exclusively by Paymob's hosted payment page.
///
/// PaymobOrderId / PaymobIntegrationId (approved additions) store the ids
/// Paymob returns when a payment is initiated, so the callback/webhook can be
/// matched back to this record and verified.
/// </summary>
public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Transaction id returned by Paymob after verification (null for COD).</summary>
    [MaxLength(256)]
    public string? TransactionId { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public DateTime? PaymentDate { get; set; }

    /// <summary>Paymob's order id from the initiate response (card payments only).</summary>
    [MaxLength(100)]
    public string? PaymobOrderId { get; set; }

    /// <summary>The Paymob integration used (card payments only).</summary>
    public int? PaymobIntegrationId { get; set; }

    // Navigation property
    public Order Order { get; set; } = null!;
}
