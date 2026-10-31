using GlowCare.API.Data;
using GlowCare.API.DTOs.Orders;
using GlowCare.API.DTOs.Promotions;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Shopping;
using GlowCare.API.Models.User;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Everything checkout needs after validation, prepared once and shared by
/// both order paths (COD: OrderService, card: PaymentService).
/// </summary>
public class CheckoutContext
{
    /// <summary>Tracked cart (with products) - callers decrement stock and clear it inside their transaction.</summary>
    public required Cart Cart { get; init; }

    /// <summary>Sum of current server-side prices.</summary>
    public decimal Subtotal { get; init; }

    /// <summary>The caller's verified address (already ownership-checked).</summary>
    public required Address Address { get; init; }

    /// <summary>Validated promo, or null when no code was submitted.</summary>
    public PromoCodeValidationDto? Promo { get; init; }

    /// <summary>The full money breakdown, computed server-side.</summary>
    public required OrderSummaryDto Summary { get; init; }

    /// <summary>Address frozen as text for the order snapshot.</summary>
    public string AddressSnapshot { get; init; } = string.Empty;
}

/// <summary>
/// Prepares a checkout for a customer: loads the cart, verifies the address
/// belongs to the caller, validates the optional promo code and computes the
/// money summary.
///
/// SINGLE SOURCE OF TRUTH: this is the ONLY place checkout money is calculated
/// (discount -> shipping -> tax -> total, all from StoreSettings). Both the
/// cash-on-delivery path and the Paymob card path call it, so the two can
/// never drift apart. Nothing here trusts a price or total from the client.
/// </summary>
public class CheckoutPreparer
{
    private readonly AppDbContext _db;
    private readonly StoreSettings _store;
    private readonly IPromoCodeService _promoCodes;

    public CheckoutPreparer(AppDbContext db, IOptions<StoreSettings> store, IPromoCodeService promoCodes)
    {
        _db = db;
        _store = store.Value;
        _promoCodes = promoCodes;
    }

    public async Task<CheckoutContext> PrepareAsync(string userId, CheckoutDto dto)
    {
        // --- cart (tracked: callers mutate stock/items in the same context) ---
        var cart = await _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || cart.Items.Count == 0)
        {
            throw new AppException("Your cart is empty.", 400);
        }

        var subtotal = cart.Items.Sum(i => i.Product.Price * i.Quantity);

        // --- address must exist AND belong to the caller ---
        var address = await _db.Addresses.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == dto.AddressId && a.UserId == userId);

        if (address is null)
        {
            throw new AppException("The selected shipping address was not found.", 404);
        }

        // --- optional promo code (validated + amount computed server-side) ---
        PromoCodeValidationDto? promo = null;

        if (!string.IsNullOrWhiteSpace(dto.PromoCode))
        {
            promo = await _promoCodes.ValidatePromoCodeAsync(userId,
                new ValidatePromoCodeDto { Code = dto.PromoCode, Subtotal = subtotal });

            if (!promo.IsValid)
            {
                throw new AppException(promo.Message, 400);
            }
        }

        // --- money: discount -> shipping -> tax -> total (StoreSettings only) ---
        var discount = promo is null ? 0m : Math.Min(promo.DiscountAmount, subtotal);
        var discountedSubtotal = subtotal - discount;

        var shipping = subtotal >= _store.FreeShippingThreshold ? 0m : _store.ShippingCost;
        var tax = Math.Round(discountedSubtotal * _store.TaxRate, 2, MidpointRounding.AwayFromZero);
        var total = discountedSubtotal + shipping + tax;

        var summary = new OrderSummaryDto
        {
            Subtotal = subtotal,
            ShippingCost = shipping,
            TaxAmount = tax,
            DiscountAmount = discount,
            TotalAmount = total,
            FreeShippingThreshold = _store.FreeShippingThreshold,
            AppliedPromoCode = promo?.Message
        };

        return new CheckoutContext
        {
            Cart = cart,
            Subtotal = subtotal,
            Address = address,
            Promo = promo,
            Summary = summary,
            AddressSnapshot = SnapshotAddress(address)
        };
    }

    /// <summary>"12 Nile St., Bldg. 4, Cairo" - frozen into the order forever.</summary>
    private static string SnapshotAddress(Address address)
        => $"{address.Street}, Bldg. {address.BuildingNumber}, {address.City}";
}
