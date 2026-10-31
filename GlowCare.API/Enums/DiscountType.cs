namespace GlowCare.API.Enums;

/// <summary>How a promo code reduces the order total.</summary>
public enum DiscountType
{
    /// <summary>Percentage of the subtotal, e.g. 10 = 10%.</summary>
    Percentage = 0,

    /// <summary>Fixed amount subtracted from the subtotal, e.g. 50 EGP.</summary>
    Fixed = 1
}
