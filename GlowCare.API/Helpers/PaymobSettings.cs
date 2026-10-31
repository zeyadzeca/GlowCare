namespace GlowCare.API.Helpers;

/// <summary>
/// Strongly typed options for the "Paymob" section of appsettings.json.
///
/// Field set verified against Paymob's official docs (Intention API +
/// Unified Checkout + HMAC transaction callbacks).
///
/// SECRETS - supplied via User Secrets or environment variables, never
/// committed to source control and never returned to the React frontend:
///   Paymob:ApiSecretKey   (Authorization: Token {key} when creating intentions)
///   Paymob:PublicKey      (builds the hosted checkout URL)
///   Paymob:HmacSecret     (verifies callback authenticity)
/// Non-secret values (BaseUrl, IntegrationId, CallbackUrl, RedirectionUrl)
/// live in appsettings.json.
/// </summary>
public class PaymobSettings
{
    public const string SectionName = "Paymob";

    /// <summary>Regional API base URL. Egypt: https://accept.paymob.com</summary>
    public string BaseUrl { get; set; } = "https://accept.paymob.com";

    /// <summary>SECRET. Paymob API secret key used to authenticate API calls.</summary>
    public string ApiSecretKey { get; set; } = string.Empty;

    /// <summary>Not secret (but per-account). Public key used in the hosted checkout URL.</summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>The card payment integration id configured in the Paymob dashboard.</summary>
    public int IntegrationId { get; set; }

    /// <summary>SECRET. HMAC-SHA512 secret used to verify callback authenticity.</summary>
    public string HmacSecret { get; set; } = string.Empty;

    /// <summary>
    /// Optional. Public URL Paymob POSTs transaction callbacks to. When empty,
    /// the webhook configured in the Paymob dashboard is used instead.
    /// </summary>
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>Optional. Where Paymob redirects the customer's browser after payment.</summary>
    public string RedirectionUrl { get; set; } = string.Empty;

    /// <summary>True only when every value required to take a card payment is present.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl) &&
        !string.IsNullOrWhiteSpace(ApiSecretKey) &&
        !string.IsNullOrWhiteSpace(PublicKey) &&
        IntegrationId > 0;
}
