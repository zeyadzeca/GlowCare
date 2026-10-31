using System.Text.Json.Serialization;

namespace GlowCare.API.DTOs.Payments;

/// <summary>
/// Outbound request for Paymob's Create Intention API
/// (POST /v1/intention/ - verified against Paymob's official docs).
/// Built ONLY from server-side order data - amounts never come from the client.
/// </summary>
public class PaymobIntentionRequestDto
{
    /// <summary>Total charge in the smallest currency unit (piastres for EGP).</summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "EGP";

    /// <summary>The integration id(s) to allow for this payment.</summary>
    [JsonPropertyName("payment_methods")]
    public int[] PaymentMethods { get; set; } = Array.Empty<int>();

    [JsonPropertyName("items")]
    public PaymobIntentionItemDto[] Items { get; set; } = Array.Empty<PaymobIntentionItemDto>();

    /// <summary>Paymob requires this set of billing fields (placeholders allowed in test).</summary>
    [JsonPropertyName("billing_data")]
    public PaymobBillingDataDto BillingData { get; set; } = new();

    /// <summary>Our own order id, so the intention can be traced back even from Paymob's side.</summary>
    [JsonPropertyName("special_reference")]
    public string SpecialReference { get; set; } = string.Empty;

    /// <summary>Optional per-intention webhook URL (omitted when not configured).</summary>
    [JsonPropertyName("notification_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NotificationUrl { get; set; }

    /// <summary>Optional browser return URL after payment (omitted when not configured).</summary>
    [JsonPropertyName("redirection_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RedirectionUrl { get; set; }
}

public class PaymobIntentionItemDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}

/// <summary>Billing fields Paymob requires on every intention (dummy values fine in test).</summary>
public class PaymobBillingDataDto
{
    [JsonPropertyName("first_name")] public string FirstName { get; set; } = string.Empty;
    [JsonPropertyName("last_name")] public string LastName { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("street")] public string Street { get; set; } = string.Empty;
    [JsonPropertyName("building")] public string Building { get; set; } = string.Empty;
    [JsonPropertyName("floor")] public string Floor { get; set; } = "1";
    [JsonPropertyName("apartment")] public string Apartment { get; set; } = "NA";
    [JsonPropertyName("city")] public string City { get; set; } = string.Empty;
    [JsonPropertyName("country")] public string Country { get; set; } = "EG";
    [JsonPropertyName("state")] public string State { get; set; } = string.Empty;
    [JsonPropertyName("phone_number")] public string PhoneNumber { get; set; } = "NA";
    [JsonPropertyName("postal_code")] public string PostalCode { get; set; } = "NA";
    [JsonPropertyName("shipping_method")] public string ShippingMethod { get; set; } = "HOME";
}
