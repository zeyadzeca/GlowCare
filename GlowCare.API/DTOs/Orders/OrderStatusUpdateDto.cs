using GlowCare.API.Enums;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Orders;

/// <summary>Request to change an order's status (Admin only).</summary>
public class OrderStatusUpdateDto
{
    [Required]
    public OrderStatus Status { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
