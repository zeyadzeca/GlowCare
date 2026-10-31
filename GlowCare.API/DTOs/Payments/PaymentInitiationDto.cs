namespace GlowCare.API.DTOs.Payments;

/// <summary>
/// Returned by the Paymob initiate endpoint. The frontend redirects the
/// customer to PaymentUrl to complete the test payment.
/// </summary>
public class PaymentInitiationDto
{
    public int OrderId { get; set; }

    public int PaymentId { get; set; }

    /// <summary>The Paymob hosted payment page URL the customer is sent to.</summary>
    public string PaymentUrl { get; set; } = string.Empty;
}
