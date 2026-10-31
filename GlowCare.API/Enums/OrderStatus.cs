namespace GlowCare.API.Enums;

/// <summary>
/// Lifecycle of an order. Managed by Admin (order-level) and reflected
/// for customers on the order tracking page.
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
