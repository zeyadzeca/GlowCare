namespace GlowCare.API.Helpers;

/// <summary>
/// strongly typed options for the "StoreSettings" section of appsettings.json.
/// These control shipping and tax behavior so values are configurable, never hardcoded.
/// </summary>
public class StoreSettings
{
    public const string SectionName = "StoreSettings";

    /// <summary>Currency symbol/label used in the UI, e.g. "EGP".</summary>
    public string Currency { get; set; } = "EGP";

    /// <summary>Orders with a subtotal above this amount get free shipping.</summary>
    public decimal FreeShippingThreshold { get; set; }

    /// <summary>Standard shipping cost applied when the threshold is not met.</summary>
    public decimal ShippingCost { get; set; }

    /// <summary>Tax rate as a decimal fraction, e.g. 0.14 for 14% VAT.</summary>
    public decimal TaxRate { get; set; }
}
