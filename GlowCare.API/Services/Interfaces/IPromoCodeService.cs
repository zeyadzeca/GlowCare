using GlowCare.API.DTOs.Promotions;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// Promo code validation and Admin management.
/// ALL discount calculations happen on the backend.
/// </summary>
public interface IPromoCodeService
{
    /// <summary>
    /// Validates a code for a user + subtotal: exists, active, not expired,
    /// usage limit not reached, minimum amount met, not already used by this
    /// user. Returns the computed discount amount when valid.
    /// </summary>
    Task<PromoCodeValidationDto> ValidatePromoCodeAsync(string userId, ValidatePromoCodeDto dto);

    /// <summary>All promo codes with usage counts (Admin only).</summary>
    Task<IList<PromoCodeDto>> GetPromoCodesAsync();

    Task<PromoCodeDto> CreateAsync(PromoCodeCreateDto dto);

    /// <summary>Returns null if the promo code does not exist.</summary>
    Task<PromoCodeDto?> UpdateAsync(int id, PromoCodeUpdateDto dto);

    /// <summary>Deactivates a promo code (soft). Returns false if not found.</summary>
    Task<bool> DeactivateAsync(int id);

    /// <summary>Records one use of a code (called inside the checkout transaction).</summary>
    Task RecordUsageAsync(int promoCodeId, string userId, int orderId);
}
