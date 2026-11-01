using GlowCare.API.DTOs.Wishlist;

namespace GlowCare.API.Services.Interfaces;

/// <summary>The authenticated customer's wishlist. No duplicates (enforced by a unique index).</summary>
public interface IWishlistService
{
    /// <summary>The user's wishlist (creates an empty one on first access).</summary>
    Task<WishlistDto> GetWishlistAsync(string userId);

    /// <summary>Saves a product to the wishlist. Idempotent (already saved = no-op).</summary>
    Task<WishlistDto> AddItemAsync(string userId, int productId);

    /// <summary>Removes one wishlist item. Returns false if not found or not owned.</summary>
    Task<bool> RemoveItemAsync(string userId, int wishlistItemId);

    /// <summary>Removes a product from the caller's wishlist. Returns false if not found or not owned.</summary>
    Task<bool> RemoveItemByProductIdAsync(string userId, int productId);
}
