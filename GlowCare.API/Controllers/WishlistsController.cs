using GlowCare.API.DTOs.Wishlist;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>The authenticated customer's wishlist. Customer role only.</summary>
[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/wishlist")]
public class WishlistsController : ControllerBase
{
    private readonly IWishlistService _wishlists;

    public WishlistsController(IWishlistService wishlists)
    {
        _wishlists = wishlists;
    }

    /// <summary>My full wishlist.</summary>
    [HttpGet]
    public async Task<ActionResult<WishlistDto>> GetWishlist()
        => Ok(await _wishlists.GetWishlistAsync(User.GetUserId()!));

    /// <summary>Saves a product (saving it twice is a no-op).</summary>
    [HttpPost("items")]
    public async Task<ActionResult<WishlistDto>> AddItem(AddWishlistItemDto dto)
        => Ok(await _wishlists.AddItemAsync(User.GetUserId()!, dto.ProductId));

    /// <summary>Removes one wishlist item.</summary>
    [HttpDelete("items/{itemId:int}")]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        if (!await _wishlists.RemoveItemAsync(User.GetUserId()!, itemId))
        {
            return NotFound(new { statusCode = 404, message = $"Wishlist item {itemId} was not found." });
        }

        return Ok(new { message = "Item removed from wishlist." });
    }
}
