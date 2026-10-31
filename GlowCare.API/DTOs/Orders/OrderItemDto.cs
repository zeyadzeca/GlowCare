using GlowCare.API.Enums;

namespace GlowCare.API.DTOs.Orders;

/// <summary>One purchased line, with the purchase-time snapshot values.</summary>
public class OrderItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;

    public string SellerId { get; set; } = string.Empty;

    public string SellerStoreName { get; set; } = string.Empty;

    public string? ProductImageUrl { get; set; }

    public FulfillmentStatus FulfillmentStatus { get; set; }
}
