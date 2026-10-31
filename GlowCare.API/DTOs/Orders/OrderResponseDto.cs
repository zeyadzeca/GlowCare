using GlowCare.API.Enums;

namespace GlowCare.API.DTOs.Orders;

/// <summary>An order as seen by its owner (customer), a seller (their items), or the admin.</summary>
public class OrderResponseDto
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public decimal Subtotal { get; set; }

    public decimal ShippingCost { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string ShippingAddress { get; set; } = string.Empty;

    public string? PromoCode { get; set; }

    public IList<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

    /// <summary>Status tracking timeline, oldest first.</summary>
    public IList<OrderStatusHistoryDto> StatusHistory { get; set; } = new List<OrderStatusHistoryDto>();
}
