using GlowCare.API.DTOs.Products;
using GlowCare.API.DTOs.Seller;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// Seller dashboard: statistics and fulfillment of MY order items.
/// sellerId always comes from JWT claims, so a seller can never see or touch
/// another seller's data. Product management lives in ProductsController.
/// </summary>
[ApiController]
[Authorize(Roles = "Seller")]
[Route("api/seller")]
public class SellerController : ControllerBase
{
    private readonly ISellerService _seller;
    private readonly IProductService _products;

    public SellerController(ISellerService seller, IProductService products)
    {
        _seller = seller;
        _products = products;
    }

    /// <summary>Product counts, pending items and earnings for MY store.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<SellerDashboardStatsDto>> GetDashboard()
        => Ok(await _seller.GetDashboardStatsAsync(User.GetUserId()!));

    /// <summary>MY products (including inactive), newest first.</summary>
    [HttpGet("products")]
    public async Task<ActionResult<IList<ProductResponseDto>>> GetMyProducts()
        => Ok(await _products.GetSellerProductsAsync(User.GetUserId()!));

    /// <summary>Order items containing MY products, newest first.</summary>
    [HttpGet("order-items")]
    [HttpGet("orders")]
    public async Task<ActionResult<IList<SellerOrderItemDto>>> GetOrderItems()
        => Ok(await _seller.GetOrderItemsAsync(User.GetUserId()!));

    /// <summary>Marks one of MY order items as fulfilled (or back to not fulfilled).</summary>
    [HttpPut("order-items/{orderItemId:int}/fulfillment")]
    public async Task<ActionResult<SellerOrderItemDto>> UpdateFulfillment(int orderItemId, FulfillmentUpdateDto dto)
    {
        var item = await _seller.UpdateFulfillmentAsync(User.GetUserId()!, orderItemId, dto.Fulfilled);

        return item is null
            ? NotFound(new { statusCode = 404, message = $"Order item {orderItemId} was not found." })
            : Ok(item);
    }
}
