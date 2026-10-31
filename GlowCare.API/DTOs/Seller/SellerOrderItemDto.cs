using GlowCare.API.Enums;

namespace GlowCare.API.DTOs.Seller;

/// <summary>
/// One of MY order items (a customer bought my product), with enough order
/// context to fulfill it. Sellers never see other sellers' items.
/// </summary>
public class SellerOrderItemDto
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public FulfillmentStatus FulfillmentStatus { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>Snapshot of the buyer's shipping address for this order.</summary>
    public string ShippingAddress { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;
}
