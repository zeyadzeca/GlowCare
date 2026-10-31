using GlowCare.API.DTOs.Orders;
using GlowCare.API.DTOs.Payments;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// Card payments via Paymob (test mode).
/// - initiate: creates a Pending order (stock held) and returns the hosted
///   payment page URL.
/// - callback: anonymous endpoint Paymob POSTs the result to - authenticity is
///   proven by the HMAC query parameter, so no JWT is possible or needed.
/// - lookup: payment status for the order's owner.
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;

    public PaymentsController(IPaymentService payments)
    {
        _payments = payments;
    }

    /// <summary>
    /// Starts a credit-card checkout: creates the Pending order in one
    /// transaction, then returns the Paymob hosted payment URL the frontend
    /// redirects the customer to. Payment becomes Paid only via the verified callback.
    /// </summary>
    [Authorize(Roles = "Customer")]
    [HttpPost("initiate")]
    [HttpPost("paymob/initiate")]
    public async Task<ActionResult<PaymentInitiationDto>> InitiatePayment(CheckoutDto dto)
        => Ok(await _payments.InitiatePaymobPaymentAsync(User.GetUserId()!, dto));

    /// <summary>
    /// Paymob transaction webhook. NOT authenticated with a JWT - instead the
    /// HMAC query parameter is recomputed server-side over the documented
    /// fields; callbacks with a bad signature are rejected with 401 and change
    /// nothing. Success here is the ONLY thing that marks an order Paid.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("callback")]
    [HttpPost("paymob/callback")]
    public async Task<IActionResult> HandleCallback([FromBody] PaymobCallbackDto callback, [FromQuery(Name = "hmac")] string? hmac)
    {
        callback.Hmac = hmac;
        await _payments.HandlePaymobCallbackAsync(callback);

        // Acknowledge so Paymob does not retry a callback we fully processed.
        return Ok();
    }

    /// <summary>One of MY payments (404 when not owned by the caller).</summary>
    [Authorize(Roles = "Customer")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentResponseDto>> GetPayment(int id)
    {
        var payment = await _payments.GetPaymentByIdAsync(User.GetUserId()!, id);

        return payment is null
            ? NotFound(new { statusCode = 404, message = $"Payment {id} was not found." })
            : Ok(payment);
    }
}
