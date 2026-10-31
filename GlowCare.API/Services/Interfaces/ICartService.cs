using GlowCare.API.DTOs.Cart;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// The authenticated customer's persistent cart.
/// There is no guest cart - every method requires a userId from JWT claims.
/// </summary>
public interface ICartService
{
    /// <summary>Gets the user's cart (creates an empty one on first access).</summary>
    Task<CartDto> GetCartAsync(string userId);

    /// <summary>Adds a product, or increases the quantity if it is already in the cart.</summary>
    Task<CartDto> AddItemAsync(string userId, AddCartItemDto dto);

    /// <summary>Sets a new quantity for one cart item. Returns null if not owned.</summary>
    Task<CartDto?> UpdateItemQuantityAsync(string userId, int cartItemId, int quantity);

    /// <summary>Removes one cart item. Returns false if not owned.</summary>
    Task<bool> RemoveItemAsync(string userId, int cartItemId);

    /// <summary>Empties the cart (used after a successful order).</summary>
    Task ClearCartAsync(string userId);
}
