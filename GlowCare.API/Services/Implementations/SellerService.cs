using GlowCare.API.Data;
using GlowCare.API.DTOs.Seller;
using GlowCare.API.Enums;
using GlowCare.API.Models.Orders;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// The seller dashboard: statistics and per-item fulfillment.
/// EVERY query is filtered by sellerId (from JWT claims) - a seller can never
/// see another seller's products, items, customers or earnings.
/// </summary>
public class SellerService : ISellerService
{
    private readonly AppDbContext _db;

    public SellerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SellerDashboardStatsDto> GetDashboardStatsAsync(string sellerId)
    {
        var products = _db.Products.AsNoTracking().Where(p => p.SellerId == sellerId);

        // Cancelled orders were restocked - they are not earnings.
        var items = _db.OrderItems.AsNoTracking()
            .Where(oi => oi.SellerId == sellerId && oi.Order.Status != OrderStatus.Cancelled);

        return new SellerDashboardStatsDto
        {
            TotalProducts = await products.CountAsync(),
            ActiveProducts = await products.CountAsync(p => p.IsActive),
            TotalOrderItems = await items.CountAsync(),
            PendingOrderItems = await items.CountAsync(oi => oi.FulfillmentStatus == FulfillmentStatus.NotFulfilled),
            TotalEarnings = await items.SumAsync(oi => oi.UnitPrice * oi.Quantity),
            ConfirmedEarnings = await items
                .Where(oi => oi.FulfillmentStatus == FulfillmentStatus.Fulfilled &&
                             oi.Order.Status == OrderStatus.Delivered)
                .SumAsync(oi => oi.UnitPrice * oi.Quantity)
        };
    }

    public async Task<IList<SellerOrderItemDto>> GetOrderItemsAsync(string sellerId)
    {
        var items = await _db.OrderItems.AsNoTracking()
            .Where(oi => oi.SellerId == sellerId)
            .OrderByDescending(oi => oi.Order.OrderDate).ThenByDescending(oi => oi.Id)
            .Include(oi => oi.Order).ThenInclude(o => o.User)
            .Include(oi => oi.Product)
            .AsSplitQuery()
            .ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task<SellerOrderItemDto?> UpdateFulfillmentAsync(string sellerId, int orderItemId, bool fulfilled)
    {
        // Tracked so the update goes through EF change tracking (and RowVersion
        // style conflicts on the item surface as clean errors upstream).
        var item = await _db.OrderItems
            .Include(oi => oi.Order).ThenInclude(o => o.User)
            .Include(oi => oi.Product)
            .AsSplitQuery()
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId && oi.SellerId == sellerId);

        if (item is null)
        {
            return null; // not found or belongs to another seller
        }

        item.FulfillmentStatus = fulfilled ? FulfillmentStatus.Fulfilled : FulfillmentStatus.NotFulfilled;
        await _db.SaveChangesAsync();

        return Map(item);
    }

    private static SellerOrderItemDto Map(OrderItem item) => new()
    {
        OrderItemId = item.Id,
        OrderId = item.OrderId,
        OrderDate = item.Order.OrderDate,
        OrderStatus = item.Order.Status,
        FulfillmentStatus = item.FulfillmentStatus,
        ProductId = item.ProductId,
        ProductName = item.ProductName,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        ShippingAddress = item.Order.ShippingAddress,
        CustomerName = item.Order.User.FullName
    };
}
