using System.Text.Json.Serialization;

namespace GlowCare.API.DTOs.Payments;

/// <summary>
/// Response of Paymob's Create Intention API (HTTP 201) - the fields we use.
/// Verified against Paymob's official docs; unknown fields are ignored.
/// </summary>
public class PaymobIntentionResponseDto
{
    /// <summary>Single-use token used to launch the hosted Unified Checkout page.</summary>
    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; set; }

    /// <summary>Paymob's order id for this intention - stored on our Payment row
    /// so the later callback (obj.order.id) can be matched back to it.</summary>
    [JsonPropertyName("intention_order_id")]
    public long? IntentionOrderId { get; set; }

    /// <summary>The intention id itself, e.g. "pi_test_...".</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Payment key details including the integration actually used.</summary>
    [JsonPropertyName("payment_keys")]
    public PaymobPaymentKeyDto[]? PaymentKeys { get; set; }
}

public class PaymobPaymentKeyDto
{
    [JsonPropertyName("integration")]
    public long? Integration { get; set; }

    [JsonPropertyName("order_id")]
    public long? OrderId { get; set; }
}
