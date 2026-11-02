using GlowCare.API.DTOs.Orders;
using GlowCare.API.DTOs.Payments;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// COD and Paymob (test mode) payments.
///
/// SECURITY RULES:
/// - The order status/payment status change ONLY after the Paymob callback is
///   verified (HMAC check) - never because the frontend redirected to a
///   "success" page.
/// - Card details never touch our servers or database.
/// - Callback processing is idempotent (re-sent callbacks do not double-apply).
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a Pending order + Payment for a credit-card checkout and calls
    /// Paymob's API to obtain the hosted payment page URL.
    /// Stock is decremented at this point (held) and the cart is cleared.
    /// </summary>
    Task<PaymentInitiationDto> InitiatePaymobPaymentAsync(string userId, CheckoutDto dto);

    /// <summary>
    /// Processes the Paymob callback/webhook: verifies authenticity (HMAC),
    /// then updates Payment and Order status. Idempotent.
    /// </summary>
    Task HandlePaymobCallbackAsync(PaymobCallbackDto dto);

    /// <summary>One payment record. Null if the payment (and its order) is not owned by the user.</summary>
    Task<PaymentResponseDto?> GetPaymentByIdAsync(string userId, int paymentId);

    /// <summary>All payments (Admin only).</summary>
    Task<IList<PaymentResponseDto>> GetAllPaymentsAsync();
}
