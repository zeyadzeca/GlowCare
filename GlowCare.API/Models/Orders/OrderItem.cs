using GlowCare.API.Enums;
using GlowCare.API.Models.Catalog;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Orders;

/// <summary>
/// One product line inside an order.
/// ProductName and UnitPrice are snapshots taken at purchase time, so order
/// history stays correct even if the product later changes or is removed.
/// SellerId identifies which marketplace seller owns this item.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [MaxLength(450)]
    public string SellerId { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    /// <summary>Per-seller fulfillment (approved addition). Multi-seller safe.</summary>
    public FulfillmentStatus FulfillmentStatus { get; set; } = FulfillmentStatus.NotFulfilled;

    // Navigation properties
    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
