using GlowCare.API.DTOs.Common;
using GlowCare.API.DTOs.Orders;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// Order placement, tracking and management.
///
/// OWNERSHIP: customer methods only touch orders whose UserId matches the
/// caller. Sellers only see/update their own OrderItems. Admin sees everything.
/// All money is calculated here - never trusted from the client.
/// </summary>
public interface IOrderService
{
    /// <summary>Preview of what an order would cost (subtotal, shipping, tax, discount, total).</summary>
    Task<OrderSummaryDto> GetOrderSummaryAsync(string userId, CheckoutDto dto);

    /// <summary>
    /// Places a COD order: validates the cart, stock and promo code, calculates
    /// totals, creates the order + items + history + payment record, decrements
    /// stock and clears the cart - all in one database transaction.
    /// </summary>
    Task<OrderResponseDto> PlaceCodOrderAsync(string userId, CheckoutDto dto);

    /// <summary>The customer's order history, newest first.</summary>
    Task<IList<OrderResponseDto>> GetMyOrdersAsync(string userId);

    /// <summary>One order with items and status history. Null if not owned by the user.</summary>
    Task<OrderResponseDto?> GetOrderByIdAsync(string userId, int orderId);

    /// <summary>Cancels an order while it is still Pending. Restocks the products.</summary>
    Task<OrderResponseDto?> CancelOrderAsync(string userId, int orderId);

    /// <summary>ALL orders (Admin only), newest first.</summary>
    Task<IList<OrderResponseDto>> GetAllOrdersAsync();

    /// <summary>Changes an order's status and appends to its history (Admin only).</summary>
    Task<OrderResponseDto?> AdminUpdateStatusAsync(int orderId, OrderStatusUpdateDto dto);
}
