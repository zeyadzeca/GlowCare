using GlowCare.API.Data;
using GlowCare.API.DTOs.Wishlist;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Wishlist;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// The authenticated customer's wishlist (one per user, created lazily).
/// Same pattern as the cart: every method is scoped to the userId from JWT
/// claims, and saving a product twice is a no-op (unique index on
/// WishlistId + ProductId is the backstop).
/// </summary>
public class WishlistService : IWishlistService
{
    private readonly AppDbContext _db;

    public WishlistService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<WishlistDto> GetWishlistAsync(string userId)
    {
        var wishlist = await GetOrCreateWishlistAsync(userId);
        return await BuildWishlistDtoAsync(wishlist.Id);
    }

    public async Task<WishlistDto> AddItemAsync(string userId, int productId)
    {
        var wishlist = await GetOrCreateWishlistAsync(userId);

        var productExists = await _db.Products.AsNoTracking()
            .AnyAsync(p => p.Id == productId);

        if (!productExists)
        {
            throw new AppException($"Product {productId} was not found.", 404);
        }

        var alreadySaved = await _db.WishlistItems.AsNoTracking()
            .AnyAsync(i => i.WishlistId == wishlist.Id && i.ProductId == productId);

        if (!alreadySaved)
        {
            _db.WishlistItems.Add(new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = productId
            });

            await _db.SaveChangesAsync();
        }

        return await BuildWishlistDtoAsync(wishlist.Id);
    }

    public async Task<bool> RemoveItemAsync(string userId, int wishlistItemId)
    {
        var item = await _db.WishlistItems
            .FirstOrDefaultAsync(i => i.Id == wishlistItemId && i.Wishlist!.UserId == userId);

        if (item is null)
        {
            return false; // not found or not yours
        }

        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private async Task<Wishlist> GetOrCreateWishlistAsync(string userId)
    {
        var wishlist = await _db.Wishlists.AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wishlist is not null)
        {
            return wishlist;
        }

        wishlist = new Wishlist { UserId = userId };
        _db.Wishlists.Add(wishlist);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Two requests raced to create the first wishlist - the unique
            // index on Wishlists.UserId means one of them won; reload it.
            _db.Entry(wishlist).State = EntityState.Detached;
            wishlist = await _db.Wishlists.AsNoTracking().FirstAsync(w => w.UserId == userId);
        }

        return wishlist;
    }

    private async Task<WishlistDto> BuildWishlistDtoAsync(int wishlistId)
    {
        var items = await _db.WishlistItems.AsNoTracking()
            .Where(i => i.WishlistId == wishlistId)
            .OrderBy(i => i.Id)
            .Select(i => new WishlistItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                BrandName = i.Product.Brand.Name,
                Price = i.Product.Price,
                MainImageUrl = i.Product.Images.OrderBy(img => img.Id).Select(img => img.ImageUrl).FirstOrDefault()
            })
            .ToListAsync();

        return new WishlistDto
        {
            Id = wishlistId,
            Items = items
        };
    }
}
