using GlowCare.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Orders;

/// <summary>
/// One entry in an order's tracking timeline.
/// Every status change appends a row here (never overwrites the past).
/// </summary>
public class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation property
    public Order Order { get; set; } = null!;
}
