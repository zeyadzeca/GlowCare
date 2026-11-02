using GlowCare.API.Data;
using GlowCare.API.DTOs.Orders;
using GlowCare.API.Enums;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Orders;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Order placement, tracking and management.
///
/// SECURITY / BUSINESS RULES:
/// - Money (discount, shipping, tax, total) is computed by the shared
///   CheckoutPreparer from server-side data - nothing price-related is
///   trusted from the client. Both order paths use it, so COD and card
///   checkouts can never disagree.
/// - Address must belong to the caller (userId from JWT claims) and is
///   snapshotted into the order as text.
/// - Checkout runs inside ONE database transaction: order + items + status
///   history + payment + stock decrement + promo usage + cart clear. Any
///   failure rolls all of it back - no partial orders, no lost stock.
/// - Stock is re-validated inside the transaction and Product.RowVersion
///   (optimistic concurrency) aborts if another checkout touched the same
///   product first - this is what prevents overselling.
/// - Customers can only cancel while the order is still Pending (restock).
/// </summary>
public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IPromoCodeService _promoCodes;
    private readonly CheckoutPreparer _checkout;

    public OrderService(AppDbContext db, IPromoCodeService promoCodes, CheckoutPreparer checkout)
    {
        _db = db;
        _promoCodes = promoCodes;
        _checkout = checkout;
    }

    // ------------------------------------------------------------------
    // Checkout
    // ------------------------------------------------------------------

    public async Task<OrderSummaryDto> GetOrderSummaryAsync(string userId, CheckoutDto dto)
    {
        // Stock/availability are re-checked at placement time - this preview
        // gives the UI the numbers it needs to render the checkout page.
        var ctx = await _checkout.PrepareAsync(userId, dto);
        return ctx.Summary;
    }

    public async Task<OrderResponseDto> PlaceCodOrderAsync(string userId, CheckoutDto dto)
    {
        if (dto.PaymentMethod != PaymentMethod.CashOnDelivery)
        {
            throw new AppException("Credit card payments are initiated through the payment endpoint.", 400);
        }

        var ctx = await _checkout.PrepareAsync(userId, dto);
        var cart = ctx.Cart;
        var summary = ctx.Summary;
        var promo = ctx.Promo;

        // --- everything below happens inside ONE transaction ---
        await using var transaction = await _db.Database.BeginTransactionAsync();

        Order order;

        try
        {
            order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Subtotal = summary.Subtotal,
                ShippingCost = summary.ShippingCost,
                TaxAmount = summary.TaxAmount,
                DiscountAmount = summary.DiscountAmount,
                TotalAmount = summary.TotalAmount,
                ShippingAddress = ctx.AddressSnapshot,
                PhoneNumber = dto.PhoneNumber,
                Status = OrderStatus.Pending,
                PaymentMethod = PaymentMethod.CashOnDelivery,
                PaymentStatus = PaymentStatus.Pending,
                PromoCodeId = promo?.PromoCodeId
            };

            foreach (var item in cart.Items)
            {
                var product = item.Product;

                // Re-validated inside the transaction (stock may have changed
                // since the read above; RowVersion catches the race on save).
                if (!product.IsActive)
                {
                    throw new AppException($"\"{product.Name}\" is no longer available.", 400);
                }

                if (product.StockQuantity < item.Quantity)
                {
                    throw new AppException(
                        $"Only {product.StockQuantity} of \"{product.Name}\" left in stock.",
                        400);
                }

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,        // snapshot
                    SellerId = product.SellerId,       // from the product, never the request
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,         // snapshot of the CURRENT price
                    FulfillmentStatus = FulfillmentStatus.NotFulfilled
                });

                product.StockQuantity -= item.Quantity; // decrement (checked above)
            }

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = OrderStatus.Pending,
                ChangedAt = DateTime.UtcNow,
                Notes = "Order placed (cash on delivery)."
            });

            order.Payments.Add(new Payment
            {
                Amount = summary.TotalAmount,
                PaymentMethod = PaymentMethod.CashOnDelivery,
                PaymentStatus = PaymentStatus.Pending
            });

            _db.Orders.Add(order);
            await SaveInsideTransactionAsync(); // gets order.Id; RowVersion conflicts -> 409

            if (promo?.PromoCodeId is int promoCodeId)
            {
                await _promoCodes.RecordUsageAsync(promoCodeId, userId, order.Id);
            }

            cart.Items.Clear();
            await SaveInsideTransactionAsync();

            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Thrown while the transaction is still open - disposing it (end of
            // method) rolls EVERYTHING back before the client sees the error.
            throw new AppException(
                "Stock for items in your cart changed while you were checking out. Please review your cart and try again.",
                409);
        }

        return (await GetOrderByIdAsync(userId, order.Id))!;
    }

    // ------------------------------------------------------------------
    // Customer
    // ------------------------------------------------------------------

    public async Task<IList<OrderResponseDto>> GetMyOrdersAsync(string userId)
    {
        var orders = await QueryOrders()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(string userId, int orderId)
    {
        var order = await QueryOrders()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order is null ? null : MapToDto(order);
    }

    public async Task<OrderResponseDto?> CancelOrderAsync(string userId, int orderId)
    {
        var order = await LoadOrderTrackedAsync(orderId);

        if (order is null || order.UserId != userId)
        {
            return null; // not found or not yours
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new AppException("Only orders that are still pending can be cancelled.", 409);
        }

        Restock(order);

        order.Status = OrderStatus.Cancelled;
        order.StatusHistory.Add(new OrderStatusHistory
        {
            Status = OrderStatus.Cancelled,
            ChangedAt = DateTime.UtcNow,
            Notes = "Cancelled by customer."
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException("The order changed while you were cancelling it. Please refresh and try again.", 409);
        }

        // Re-fetch with full includes so the response carries promo code,
        // images and history exactly like a normal read would.
        return await GetOrderByIdAsync(userId, orderId);
    }

    // ------------------------------------------------------------------
    // Admin
    // ------------------------------------------------------------------

    public async Task<IList<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await QueryOrders()
            .OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderResponseDto?> AdminUpdateStatusAsync(int orderId, OrderStatusUpdateDto dto)
    {
        var order = await LoadOrderTrackedAsync(orderId);

        if (order is null)
        {
            return null;
        }

        if (order.Status == OrderStatus.Cancelled && dto.Status != OrderStatus.Cancelled)
        {
            throw new AppException("A cancelled order cannot be reactivated. Create a new order instead.", 409);
        }

        if (dto.Status != order.Status)
        {
            // Cancelling (from any state) returns the products to stock exactly
            // like a customer cancel would - no stock is ever lost.
            if (dto.Status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                Restock(order);
            }

            // COD convention: cash is collected when the order is delivered,
            // so the payment becomes Paid at that moment.
            if (dto.Status == OrderStatus.Delivered
                && order.PaymentMethod == PaymentMethod.CashOnDelivery
                && order.PaymentStatus == PaymentStatus.Pending)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                foreach (var payment in order.Payments.Where(p => p.PaymentStatus == PaymentStatus.Pending))
                {
                    payment.PaymentStatus = PaymentStatus.Paid;
                    payment.PaymentDate = DateTime.UtcNow;
                }
            }

            order.Status = dto.Status;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = dto.Status,
                ChangedAt = DateTime.UtcNow,
                Notes = dto.Notes
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new AppException("The order changed while you were updating it. Please refresh and try again.", 409);
            }
        }

        // Re-read with full includes so the admin sees the definitive state
        // (including any automatic COD payment update above).
        var refreshed = await QueryOrders().FirstOrDefaultAsync(o => o.Id == orderId);
        return refreshed is null ? null : MapToDto(refreshed);
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    /// <summary>Returns product stock to the shelf (customer cancel / admin cancel).</summary>
    private static void Restock(Order order)
    {
        foreach (var item in order.OrderItems)
        {
            item.Product.StockQuantity += item.Quantity;
        }
    }

    /// <summary>Read-only query with everything the DTO needs (split to avoid cartesian loads).</summary>
    private IQueryable<Order> QueryOrders()
        => _db.Orders.AsNoTracking()
            .Include(o => o.PromoCode)
            .Include(o => o.StatusHistory)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Images)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Seller).ThenInclude(u => u.SellerProfile)
            .AsSplitQuery();

    /// <summary>Tracked load so mutations (cancel / admin status) go through EF change tracking.</summary>
    private async Task<Order?> LoadOrderTrackedAsync(int orderId)
        => await _db.Orders
            .Include(o => o.StatusHistory)
            .Include(o => o.Payments)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId);

    private async Task SaveInsideTransactionAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Converts into AppException(409); the open transaction is rolled
            // back when the scope disposes.
            throw new AppException(
                "Stock for items in your cart changed while you were checking out. Please review your cart and try again.",
                409);
        }
    }

    private static OrderResponseDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        OrderDate = order.OrderDate,
        Status = order.Status,
        PaymentMethod = order.PaymentMethod,
        PaymentStatus = order.PaymentStatus,
        Subtotal = order.Subtotal,
        ShippingCost = order.ShippingCost,
        TaxAmount = order.TaxAmount,
        DiscountAmount = order.DiscountAmount,
        TotalAmount = order.TotalAmount,
        ShippingAddress = order.ShippingAddress,
        PhoneNumber = order.PhoneNumber,
        PromoCode = order.PromoCode?.Code,
        Items = order.OrderItems.Select(oi => new OrderItemDto
        {
            Id = oi.Id,
            ProductId = oi.ProductId,
            ProductName = oi.ProductName,
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice,
            SellerId = oi.SellerId,
            SellerStoreName = oi.Product?.Seller?.SellerProfile?.StoreName ?? string.Empty,
            ProductImageUrl = oi.Product != null
                ? oi.Product.Images.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault()
                : null,
            FulfillmentStatus = oi.FulfillmentStatus
        }).ToList(),
        StatusHistory = order.StatusHistory
            .OrderBy(h => h.ChangedAt).ThenBy(h => h.Id)
            .Select(h => new OrderStatusHistoryDto
            {
                Status = h.Status,
                ChangedAt = h.ChangedAt,
                Notes = h.Notes
            })
            .ToList()
    };
}
