using GlowCare.API.DTOs.Orders;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// Admin order management: view all orders and move them through their status
/// timeline. Customer-facing order endpoints live in OrdersController.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public AdminOrdersController(IOrderService orders)
    {
        _orders = orders;
    }

    /// <summary>ALL orders across all customers, newest first.</summary>
    [HttpGet]
    public async Task<ActionResult<IList<OrderResponseDto>>> GetAllOrders()
        => Ok(await _orders.GetAllOrdersAsync());

    /// <summary>
    /// Changes an order's status (Processing / Shipped / Delivered / Cancelled)
    /// and appends to its tracking timeline. Cancelling restocks the products.
    /// </summary>
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<OrderResponseDto>> UpdateOrderStatus(int id, OrderStatusUpdateDto dto)
    {
        var order = await _orders.AdminUpdateStatusAsync(id, dto);

        return order is null
            ? NotFound(new { statusCode = 404, message = $"Order {id} was not found." })
            : Ok(order);
    }
}
