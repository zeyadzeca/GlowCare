using GlowCare.API.Data;
using GlowCare.API.DTOs.Cart;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Shopping;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// The authenticated customer's persistent cart (one cart per user, created
/// lazily on first access). No guest cart by design.
/// Every method is scoped to the userId from JWT claims - a customer can only
/// ever touch their own cart. All money values come from the Product's current
/// server-side price (LineTotal is computed, never sent by the client).
/// </summary>
public class CartService : ICartService
{
    private readonly AppDbContext _db;

    public CartService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await BuildCartDtoAsync(cart.Id);
    }

    public async Task<CartDto> AddItemAsync(string userId, AddCartItemDto dto)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var product = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product is null || !product.IsActive)
        {
            throw new AppException($"Product {dto.ProductId} is not available.", 404);
        }

        var existingItem = await _db.CartItems
            .FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == dto.ProductId);

        // Adding an existing line just increases its quantity - stock applies to the total.
        var newQuantity = (existingItem?.Quantity ?? 0) + dto.Quantity;

        if (newQuantity > product.StockQuantity)
        {
            throw new AppException(
                product.StockQuantity == 0
                    ? $"\"{product.Name}\" is out of stock."
                    : $"Only {product.StockQuantity} of \"{product.Name}\" available.",
                400);
        }

        if (existingItem is not null)
        {
            existingItem.Quantity = newQuantity;
        }
        else
        {
            _db.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = dto.Quantity
            });
        }

        await _db.SaveChangesAsync();
        return await BuildCartDtoAsync(cart.Id);
    }

    public async Task<CartDto?> UpdateItemQuantityAsync(string userId, int cartItemId, int quantity)
    {
        var item = await FindOwnedItemAsync(userId, cartItemId);

        if (item is null)
        {
            return null; // not found or not yours
        }

        if (quantity > item.Product.StockQuantity)
        {
            throw new AppException(
                item.Product.StockQuantity == 0
                    ? $"\"{item.Product.Name}\" is out of stock."
                    : $"Only {item.Product.StockQuantity} of \"{item.Product.Name}\" available.",
                400);
        }

        item.Quantity = quantity;
        await _db.SaveChangesAsync();

        return await BuildCartDtoAsync(item.CartId);
    }

    public async Task<bool> RemoveItemAsync(string userId, int cartItemId)
    {
        var item = await FindOwnedItemAsync(userId, cartItemId);

        if (item is null)
        {
            return false;
        }

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null)
        {
            return; // no cart yet - nothing to clear
        }

        cart.Items.Clear();
        await _db.SaveChangesAsync();
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _db.Carts.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { UserId = userId };
        _db.Carts.Add(cart);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Two requests raced to create the first cart - the unique index on
            // Carts.UserId means the other request won; just reload it.
            _db.Entry(cart).State = EntityState.Detached;
            cart = await _db.Carts.AsNoTracking().FirstAsync(c => c.UserId == userId);
        }

        return cart;
    }

    /// <summary>Looks up a cart item and verifies it belongs to the caller.</summary>
    private async Task<CartItem?> FindOwnedItemAsync(string userId, int cartItemId)
        => await _db.CartItems
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart!.UserId == userId);

    /// <summary>Builds the response with fresh product data and the server-side subtotal.</summary>
    private async Task<CartDto> BuildCartDtoAsync(int cartId)
    {
        var items = await _db.CartItems.AsNoTracking()
            .Where(i => i.CartId == cartId)
            .OrderBy(i => i.Id)
            .Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                BrandName = i.Product.Brand.Name,
                MainImageUrl = i.Product.Images.OrderBy(img => img.Id).Select(img => img.ImageUrl).FirstOrDefault(),
                UnitPrice = i.Product.Price,
                Quantity = i.Quantity,
                AvailableStock = i.Product.StockQuantity
            })
            .ToListAsync();

        return new CartDto
        {
            Id = cartId,
            Items = items,
            Subtotal = items.Sum(i => i.LineTotal),
            TotalItems = items.Sum(i => i.Quantity)
        };
    }
}
