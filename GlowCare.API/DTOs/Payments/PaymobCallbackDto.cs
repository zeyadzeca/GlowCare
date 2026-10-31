using System.Text.Json.Serialization;

namespace GlowCare.API.DTOs.Payments;

/// <summary>
/// The exact transaction callback Paymob POSTs after a payment succeeds or is
/// declined (verified against Paymob's official "HMAC Transaction Callback"
/// docs - shape and field names confirmed there).
///
/// Field names are snake_case exactly as Paymob sends them, hence the explicit
/// [JsonPropertyName] attributes (System.Text.Json does not map underscores by
/// itself).
///
/// The HMAC arrives as a QUERY parameter (?hmac=...), not in the body - the
/// controller binds it into <see cref="Hmac"/> before calling the service.
/// </summary>
public class PaymobCallbackDto
{
    /// <summary>Callback type, e.g. "TRANSACTION".</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>The transaction details.</summary>
    [JsonPropertyName("obj")]
    public PaymobTransactionDto? Obj { get; set; }

    /// <summary>
    /// HMAC-SHA512 digest from the query string. Bound by the controller -
    /// used to verify the callback really came from Paymob.
    /// </summary>
    public string? Hmac { get; set; }
}

/// <summary>The "obj" payload of a transaction callback.</summary>
public class PaymobTransactionDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("pending")]
    public bool? Pending { get; set; }

    /// <summary>Amount in the smallest currency unit (piastres for EGP).</summary>
    [JsonPropertyName("amount_cents")]
    public long? AmountCents { get; set; }

    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("is_auth")]
    public bool? IsAuth { get; set; }

    [JsonPropertyName("is_capture")]
    public bool? IsCapture { get; set; }

    [JsonPropertyName("is_standalone_payment")]
    public bool? IsStandalonePayment { get; set; }

    [JsonPropertyName("is_voided")]
    public bool? IsVoided { get; set; }

    [JsonPropertyName("is_refunded")]
    public bool? IsRefunded { get; set; }

    [JsonPropertyName("is_3d_secure")]
    public bool? Is3dSecure { get; set; }

    [JsonPropertyName("integration_id")]
    public long? IntegrationId { get; set; }

    [JsonPropertyName("has_parent_transaction")]
    public bool? HasParentTransaction { get; set; }

    /// <summary>Paymob's order reference - matched against Payment.PaymobOrderId.</summary>
    [JsonPropertyName("order")]
    public PaymobOrderRefDto? Order { get; set; }

    /// <summary>Original creation timestamp STRING - used verbatim in the HMAC string.</summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("source_data")]
    public PaymobSourceDataDto? SourceData { get; set; }

    [JsonPropertyName("owner")]
    public long? Owner { get; set; }

    [JsonPropertyName("error_occured")]
    public bool? ErrorOccured { get; set; }
}

/// <summary>Paymob order reference inside the callback.</summary>
public class PaymobOrderRefDto
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }
}

/// <summary>Masked card info (last digits + scheme only - never a full PAN).</summary>
public class PaymobSourceDataDto
{
    [JsonPropertyName("pan")]
    public string? Pan { get; set; }

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
