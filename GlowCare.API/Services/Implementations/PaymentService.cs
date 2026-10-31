using GlowCare.API.Data;
using GlowCare.API.DTOs.Orders;
using GlowCare.API.DTOs.Payments;
using GlowCare.API.Enums;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Identity;
using GlowCare.API.Models.Orders;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Paymob (test mode) card payments - Intention API + Unified Checkout,
/// flow verified against Paymob's official documentation.
///
/// SECURITY RULES:
/// - The order/payment becomes Paid ONLY from the HMAC-verified callback -
///   never from the browser redirect, which is pure UX.
/// - Callbacks are verified with HMAC-SHA512 over the exact documented field
///   order; a bad signature changes NOTHING.
/// - Callback processing is idempotent: a re-sent callback is acknowledged but
///   never applied twice (no double stock restock, no status flip-flop).
/// - Card details never touch our servers - Paymob's hosted page collects them.
/// - Order creation uses the same ONE transaction as COD (stock held, promo
///   recorded, cart cleared); if Paymob rejects the intention, everything rolls
///   back - no orphan orders.
/// </summary>
public class PaymentService : IPaymentService
{
    private const string HttpClientName = "Paymob";

    private readonly AppDbContext _db;
    private readonly PaymobSettings _paymob;
    private readonly StoreSettings _store;
    private readonly IPromoCodeService _promoCodes;
    private readonly CheckoutPreparer _checkout;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        AppDbContext db,
        IOptions<PaymobSettings> paymob,
        IOptions<StoreSettings> store,
        IPromoCodeService promoCodes,
        CheckoutPreparer checkout,
        IHttpClientFactory httpClientFactory,
        ILogger<PaymentService> logger)
    {
        _db = db;
        _paymob = paymob.Value;
        _store = store.Value;
        _promoCodes = promoCodes;
        _checkout = checkout;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // ------------------------------------------------------------------
    // Initiate a card payment
    // ------------------------------------------------------------------

    public async Task<PaymentInitiationDto> InitiatePaymobPaymentAsync(string userId, CheckoutDto dto)
    {
        if (dto.PaymentMethod != PaymentMethod.CreditCard)
        {
            throw new AppException("Use the orders endpoint for cash-on-delivery orders.", 400);
        }

        if (!_paymob.IsConfigured)
        {
            // Dev machines without credentials get a clear message instead of a
            // mysterious 500. Secrets go into User Secrets (see docs comment).
            throw new AppException(
                "Card payments are not configured. Set Paymob:ApiSecretKey, Paymob:PublicKey and Paymob:IntegrationId (dotnet user-secrets set ...).",
                503);
        }

        // Shared checkout: cart, address ownership, promo, money - identical to COD.
        var ctx = await _checkout.PrepareAsync(userId, dto);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        Order order;
        Payment payment;

        try
        {
            order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Subtotal = ctx.Summary.Subtotal,
                ShippingCost = ctx.Summary.ShippingCost,
                TaxAmount = ctx.Summary.TaxAmount,
                DiscountAmount = ctx.Summary.DiscountAmount,
                TotalAmount = ctx.Summary.TotalAmount,
                ShippingAddress = ctx.AddressSnapshot,
                Status = OrderStatus.Pending,          // stays Pending until the callback confirms
                PaymentMethod = PaymentMethod.CreditCard,
                PaymentStatus = PaymentStatus.Pending, // becomes Paid ONLY via verified callback
                PromoCodeId = ctx.Promo?.PromoCodeId
            };

            foreach (var item in ctx.Cart.Items)
            {
                var product = item.Product;

                // Re-validated inside the transaction; RowVersion catches races.
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
                    ProductName = product.Name,
                    SellerId = product.SellerId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    FulfillmentStatus = FulfillmentStatus.NotFulfilled
                });

                product.StockQuantity -= item.Quantity; // stock HELD for this payment
            }

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = OrderStatus.Pending,
                ChangedAt = DateTime.UtcNow,
                Notes = "Card payment initiated (Paymob)."
            });

            payment = new Payment
            {
                Amount = ctx.Summary.TotalAmount,
                PaymentMethod = PaymentMethod.CreditCard,
                PaymentStatus = PaymentStatus.Pending
            };
            order.Payments.Add(payment);

            _db.Orders.Add(order);
            await SaveInsideTransactionAsync(); // order.Id + payment.Id generated

            if (ctx.Promo?.PromoCodeId is int promoCodeId)
            {
                await _promoCodes.RecordUsageAsync(promoCodeId, userId, order.Id);
            }

            ctx.Cart.Items.Clear();
            await SaveInsideTransactionAsync();

            // --- call Paymob while the transaction is still open ---
            // If Paymob rejects the request we throw, the transaction disposes
            // and the whole order/stock/cart changes roll back.
            var user = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
            var intention = await CreateIntentionAsync(order, user, ctx);

            if (intention.IntentionOrderId is null)
            {
                // Without Paymob's order id the later callback could never be
                // matched back to this payment - treat as a gateway failure.
                _logger.LogError("Paymob intention response missing intention_order_id.");
                throw new AppException("The payment gateway returned an unexpected response.", 502);
            }

            payment.PaymobOrderId = intention.IntentionOrderId.Value.ToString(CultureInfo.InvariantCulture);
            payment.PaymobIntegrationId = _paymob.IntegrationId;
            await SaveInsideTransactionAsync();

            await transaction.CommitAsync();

            var paymentUrl = BuildCheckoutUrl(intention.ClientSecret!);

            return new PaymentInitiationDto
            {
                OrderId = order.Id,
                PaymentId = payment.Id,
                PaymentUrl = paymentUrl
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException(
                "Stock for items in your cart changed while you were checking out. Please review your cart and try again.",
                409);
        }
    }

    // ------------------------------------------------------------------
    // Callback (the ONLY source of truth for payment status)
    // ------------------------------------------------------------------

    public async Task HandlePaymobCallbackAsync(PaymobCallbackDto dto)
    {
        if (string.IsNullOrWhiteSpace(_paymob.HmacSecret))
        {
            throw new AppException("Card payments are not configured (missing Paymob:HmacSecret).", 503);
        }

        if (dto.Obj is null)
        {
            throw new AppException("Callback payload is missing transaction data.", 400);
        }

        // --- 1. Authenticity: recompute the HMAC and compare (constant time) ---
        if (!IsValidHmac(dto))
        {
            // NOTHING is changed when the signature does not match.
            _logger.LogWarning(
                "Rejected Paymob callback with invalid HMAC (transaction {TxId}).",
                dto.Obj.Id);

            throw new AppException("Invalid callback signature.", 401);
        }

        // --- 2. Match the callback to OUR payment record ---
        var paymobOrderId = dto.Obj.Order?.Id?.ToString(CultureInfo.InvariantCulture);

        if (string.IsNullOrEmpty(paymobOrderId))
        {
            throw new AppException("Callback is missing the order reference.", 400);
        }

        var payment = await _db.Payments
            .Include(p => p.Order).ThenInclude(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Include(p => p.Order).ThenInclude(o => o.StatusHistory)
            .FirstOrDefaultAsync(p => p.PaymobOrderId == paymobOrderId);

        if (payment is null)
        {
            // Nothing to update (our transaction may have rolled back after the
            // intention was created). ACK with no state change so Paymob stops retrying.
            _logger.LogWarning("Paymob callback for unknown order reference {OrderId}.", paymobOrderId);
            return;
        }

        // --- 3. Idempotency: a terminal payment is never re-applied ---
        if (payment.PaymentStatus is PaymentStatus.Paid or PaymentStatus.Failed)
        {
            _logger.LogInformation(
                "Duplicate Paymob callback ignored for payment {PaymentId} (already {Status}).",
                payment.Id, payment.PaymentStatus);
            return;
        }

        // --- 4. Sanity: amount must match what we asked for (piastres) ---
        var expectedCents = (long)Math.Round(payment.Amount * 100m, 0, MidpointRounding.AwayFromZero);

        if (dto.Obj.AmountCents is long actualCents && actualCents != expectedCents)
        {
            _logger.LogError(
                "Paymob callback amount mismatch for payment {PaymentId}: expected {Expected}, got {Actual}.",
                payment.Id, expectedCents, actualCents);

            throw new AppException("Callback amount does not match the payment.", 400);
        }

        var order = payment.Order;

        if (dto.Obj.Success == true && dto.Obj.ErrorOccured != true)
        {
            // --- SUCCESS ---
            payment.PaymentStatus = PaymentStatus.Paid;
            payment.PaymentDate = DateTime.UtcNow;
            payment.TransactionId = dto.Obj.Id?.ToString(CultureInfo.InvariantCulture);

            order.PaymentStatus = PaymentStatus.Paid;
            // Order.Status stays Pending - the admin moves it along the timeline.
        }
        else
        {
            // --- FAILED / DECLINED ---
            payment.PaymentStatus = PaymentStatus.Failed;
            payment.TransactionId = dto.Obj.Id?.ToString(CultureInfo.InvariantCulture);
            order.PaymentStatus = PaymentStatus.Failed;

            // Restock a still-pending order (agreed rule: restock on failed payment).
            if (order.Status == OrderStatus.Pending)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Product.StockQuantity += item.Quantity;
                }

                order.Status = OrderStatus.Cancelled;
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    Status = OrderStatus.Cancelled,
                    ChangedAt = DateTime.UtcNow,
                    Notes = "Payment failed - order cancelled and stock restored."
                });
            }
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Non-200 answer -> Paymob retries later; state stayed unchanged.
            throw new AppException("Payment record changed while processing the callback.", 409);
        }
    }

    public async Task<PaymentResponseDto?> GetPaymentByIdAsync(string userId, int paymentId)
    {
        var payment = await _db.Payments.AsNoTracking()
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment is null || payment.Order.UserId != userId)
        {
            return null; // not found or not yours
        }

        return new PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            PaymentDate = payment.PaymentDate,
            TransactionId = payment.TransactionId
        };
    }

    // ------------------------------------------------------------------
    // Paymob API calls
    // ------------------------------------------------------------------

    /// <summary>
    /// POST {BaseUrl}/v1/intention/ with "Authorization: Token {ApiSecretKey}".
    /// Returns the client_secret used to open the hosted checkout page.
    /// </summary>
    private async Task<PaymobIntentionResponseDto> CreateIntentionAsync(
        Order order, ApplicationUser user, CheckoutContext ctx)
    {
        var amountCents = (long)Math.Round(order.TotalAmount * 100m, 0, MidpointRounding.AwayFromZero);

        var request = new PaymobIntentionRequestDto
        {
            Amount = amountCents,
            Currency = _store.Currency,
            PaymentMethods = new[] { _paymob.IntegrationId },
            // One line item for the full amount: Paymob requires item amounts to
            // sum exactly to the total (itemized lines would need discount and
            // rounding adjustments to still match).
            Items = new[]
            {
                new PaymobIntentionItemDto
                {
                    Name = $"GlowCare Order #{order.Id}",
                    Amount = amountCents,
                    Description = "GlowCare order",
                    Quantity = 1
                }
            },
            BillingData = BuildBillingData(user, ctx),
            SpecialReference = order.Id.ToString(CultureInfo.InvariantCulture),
            NotificationUrl = string.IsNullOrWhiteSpace(_paymob.CallbackUrl) ? null : _paymob.CallbackUrl,
            RedirectionUrl = string.IsNullOrWhiteSpace(_paymob.RedirectionUrl) ? null : _paymob.RedirectionUrl
        };

        var client = _httpClientFactory.CreateClient(HttpClientName);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Token", _paymob.ApiSecretKey);

        var url = $"{_paymob.BaseUrl.TrimEnd('/')}/v1/intention/";
        var response = await client.PostAsJsonAsync(url, request);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Paymob intention call failed ({Status}): {Body}",
                response.StatusCode, Truncate(body, 500));

            throw new AppException(
                $"The payment gateway rejected the request ({(int)response.StatusCode}). Please try again later.",
                502);
        }

        var intention = JsonSerializer.Deserialize<PaymobIntentionResponseDto>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (intention is null || string.IsNullOrWhiteSpace(intention.ClientSecret))
        {
            _logger.LogError("Paymob intention response missing client_secret: {Body}", Truncate(body, 500));
            throw new AppException("The payment gateway returned an unexpected response.", 502);
        }

        return intention;
    }

    /// <summary>
    /// Hosted Unified Checkout URL:
    /// {BaseUrl}/unifiedcheckout/?publicKey={PublicKey}&clientSecret={client_secret}
    /// (verified against Paymob docs; client secret is single-use).
    /// </summary>
    private string BuildCheckoutUrl(string clientSecret)
        => $"{_paymob.BaseUrl.TrimEnd('/')}/unifiedcheckout/" +
           $"?publicKey={Uri.EscapeDataString(_paymob.PublicKey)}" +
           $"&clientSecret={Uri.EscapeDataString(clientSecret)}";

    /// <summary>Paymob requires the full billing_data set; placeholders are fine in test mode.</summary>
    private static PaymobBillingDataDto BuildBillingData(ApplicationUser user, CheckoutContext ctx)
    {
        var fullName = string.IsNullOrWhiteSpace(user.FullName) ? "GlowCare Customer" : user.FullName.Trim();
        var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        return new PaymobBillingDataDto
        {
            FirstName = parts[0],
            LastName = parts.Length > 1 ? parts[1] : "Customer",
            Email = user.Email ?? "customer@glowcare.com",
            Street = ctx.Address.Street,
            Building = ctx.Address.BuildingNumber,
            City = ctx.Address.City,
            State = ctx.Address.City,
            Country = "EG",
            PhoneNumber = string.IsNullOrWhiteSpace(user.PhoneNumber) ? "NA" : user.PhoneNumber!
        };
    }

    // ------------------------------------------------------------------
    // HMAC verification (Paymob "HMAC Transaction Callback" documentation)
    // ------------------------------------------------------------------

    /// <summary>
    /// Recreates the HMAC Paymob sent and compares it (constant time) with the
    /// value in the query string.
    ///
    /// Steps from the docs:
    /// 1. Take the 20 fields below IN THIS EXACT ORDER.
    /// 2. Concatenate their values as strings (booleans lowercase "true"/"false").
    /// 3. HMAC-SHA512 the concatenated string with the HMAC secret.
    /// 4. Compare with the received hmac (case-insensitive).
    /// </summary>
    private bool IsValidHmac(PaymobCallbackDto dto)
    {
        var received = (dto.Hmac ?? string.Empty).Trim();

        if (received.Length == 0 || dto.Obj is null)
        {
            return false;
        }

        var obj = dto.Obj;

        var concatenated = string.Concat(
            Number(obj.AmountCents),
            obj.CreatedAt ?? string.Empty,          // original string, verbatim
            obj.Currency ?? string.Empty,
            Bool(obj.ErrorOccured),
            Bool(obj.HasParentTransaction),
            Number(obj.Id),                          // obj.id
            Number(obj.IntegrationId),
            Bool(obj.Is3dSecure),
            Bool(obj.IsAuth),
            Bool(obj.IsCapture),
            Bool(obj.IsRefunded),
            Bool(obj.IsStandalonePayment),
            Bool(obj.IsVoided),
            Number(obj.Order?.Id),                   // order.id
            Number(obj.Owner),
            Bool(obj.Pending),
            obj.SourceData?.Pan ?? string.Empty,
            obj.SourceData?.SubType ?? string.Empty,
            obj.SourceData?.Type ?? string.Empty,
            Bool(obj.Success));

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_paymob.HmacSecret));
        var computed = Convert.ToHexString(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated))).ToLowerInvariant();

        var computedBytes = Encoding.UTF8.GetBytes(computed);
        var receivedBytes = Encoding.UTF8.GetBytes(received.ToLowerInvariant());

        // FixedTimeEquals avoids timing side channels; length check first since
        // it throws on mismatched lengths.
        return computedBytes.Length == receivedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(computedBytes, receivedBytes);
    }

    private static string Bool(bool? value) => value == true ? "true" : "false";

    private static string Number(long? value)
        => value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private async Task SaveInsideTransactionAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException(
                "Stock for items in your cart changed while you were checking out. Please review your cart and try again.",
                409);
        }
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "...";
}
