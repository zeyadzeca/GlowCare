namespace GlowCare.Import.Enums;

/// <summary>
/// Payment state of an order.
/// COD: starts Pending, becomes Paid after successful collection.
/// Credit card: becomes Paid only after Paymob callback verification.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2
}
