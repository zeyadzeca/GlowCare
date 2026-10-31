namespace GlowCare.API.DTOs.Orders;

/// <summary>
/// Order cost breakdown shown at checkout (and during the preview step).
/// All values are calculated by the backend.
/// </summary>
public class OrderSummaryDto
{
    public decimal Subtotal { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    /// <summary>Effective free-shipping threshold from StoreSettings (for UI messaging).</summary>
    public decimal FreeShippingThreshold { get; set; }

    /// <summary>Null when no valid promo code was applied; otherwise the applied discount message/code.</summary>
    public string? AppliedPromoCode { get; set; }
}
