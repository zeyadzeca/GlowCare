using GlowCare.API.Enums;

namespace GlowCare.API.DTOs.Orders;

/// <summary>One entry in an order's tracking timeline.</summary>
public class OrderStatusHistoryDto
{
    public OrderStatus Status { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Notes { get; set; }
}
