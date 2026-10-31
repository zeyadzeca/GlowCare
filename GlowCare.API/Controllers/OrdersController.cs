using GlowCare.API.DTOs.Orders;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// Customer order flow: cost preview, cash-on-delivery placement, history,
/// detail and cancellation. Customer role only; the userId always comes from
/// JWT claims, so a customer can only ever see or cancel their own orders.
/// </summary>
[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders)
    {
        _orders = orders;
    }

    /// <summary>
    /// Checkout preview: subtotal, discount, shipping, tax and total - all
    /// calculated by the backend from StoreSettings (no client-side money).
    /// </summary>
    [HttpPost("preview")]
    public async Task<ActionResult<OrderSummaryDto>> GetOrderSummary(CheckoutDto dto)
        => Ok(await _orders.GetOrderSummaryAsync(User.GetUserId()!, dto));

    /// <summary>
    /// Places a cash-on-delivery order: validates address/cart/stock/promo,
    /// creates the order, decrements stock and clears the cart in one
    /// transaction. For credit cards use the payment initiation endpoint (Phase 8c).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> PlaceOrder(CheckoutDto dto)
    {
        var order = await _orders.PlaceCodOrderAsync(User.GetUserId()!, dto);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>My order history, newest first.</summary>
    [HttpGet]
    public async Task<ActionResult<IList<OrderResponseDto>>> GetMyOrders()
        => Ok(await _orders.GetMyOrdersAsync(User.GetUserId()!));

    /// <summary>One of my orders with items and status timeline (404 if not mine).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
    {
        var order = await _orders.GetOrderByIdAsync(User.GetUserId()!, id);

        return order is null
            ? NotFound(new { statusCode = 404, message = $"Order {id} was not found." })
            : Ok(order);
    }

    /// <summary>Cancels my order while it is still Pending (products are restocked).</summary>
    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<OrderResponseDto>> CancelOrder(int id)
    {
        var order = await _orders.CancelOrderAsync(User.GetUserId()!, id);

        return order is null
            ? NotFound(new { statusCode = 404, message = $"Order {id} was not found." })
            : Ok(order);
    }
}
