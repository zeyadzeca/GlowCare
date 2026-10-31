using GlowCare.API.DTOs.Cart;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// The authenticated customer's cart. Customer role only - there is no guest
/// cart. The user id always comes from JWT claims, so a customer can only ever
/// touch their own cart.
/// </summary>
[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/cart")]
public class CartsController : ControllerBase
{
    private readonly ICartService _carts;

    public CartsController(ICartService carts)
    {
        _carts = carts;
    }

    /// <summary>The caller's full cart with the server-calculated subtotal.</summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
        => Ok(await _carts.GetCartAsync(User.GetUserId()!));

    /// <summary>Adds a product, or increases its quantity when already in the cart.</summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemDto dto)
        => Ok(await _carts.AddItemAsync(User.GetUserId()!, dto));

    /// <summary>Sets a new quantity for one of MY cart items.</summary>
    [HttpPut("items/{cartItemId:int}")]
    public async Task<ActionResult<CartDto>> UpdateItemQuantity(int cartItemId, UpdateCartItemDto dto)
    {
        var cart = await _carts.UpdateItemQuantityAsync(User.GetUserId()!, cartItemId, dto.Quantity);

        return cart is null
            ? NotFound(new { statusCode = 404, message = $"Cart item {cartItemId} was not found." })
            : Ok(cart);
    }

    /// <summary>Removes one item from MY cart.</summary>
    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        if (!await _carts.RemoveItemAsync(User.GetUserId()!, cartItemId))
        {
            return NotFound(new { statusCode = 404, message = $"Cart item {cartItemId} was not found." });
        }

        return Ok(new { message = "Item removed from cart." });
    }

    /// <summary>Empties the cart.</summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _carts.ClearCartAsync(User.GetUserId()!);
        return Ok(new { message = "Cart cleared." });
    }
}
