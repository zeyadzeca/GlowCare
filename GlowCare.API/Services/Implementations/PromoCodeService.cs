using GlowCare.API.Data;
using GlowCare.API.DTOs.Promotions;
using GlowCare.API.Enums;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Promotions;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Promo code validation (used during checkout) and Admin management.
/// ALL discount amounts are computed here - the client only ever sends the code.
///
/// Usage rules enforced (in order):
/// 1. code exists  2. IsActive  3. not expired  4. usage limit not reached
/// 5. minimum order amount met  6. this customer has not used it before
/// (rule 6 also has a unique index at the database level as a backstop).
/// </summary>
public class PromoCodeService : IPromoCodeService
{
    private readonly AppDbContext _db;

    public PromoCodeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PromoCodeValidationDto> ValidatePromoCodeAsync(string userId, ValidatePromoCodeDto dto)
    {
        var code = dto.Code.Trim();
        var subtotal = dto.Subtotal;

        var promo = await _db.PromoCodes.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code.ToLower() == code.ToLower());

        if (promo is null)
        {
            return Invalid("Promo code not found.");
        }

        if (!promo.IsActive)
        {
            return Invalid("This promo code is no longer active.");
        }

        if (promo.ExpiryDate <= DateTime.UtcNow)
        {
            return Invalid("This promo code has expired.");
        }

        var timesUsed = await _db.PromoCodeUsages.AsNoTracking()
            .CountAsync(u => u.PromoCodeId == promo.Id);

        if (timesUsed >= promo.UsageLimit)
        {
            return Invalid("This promo code has reached its usage limit.");
        }

        if (subtotal < promo.MinimumOrderAmount)
        {
            return Invalid(
                $"A minimum order of {promo.MinimumOrderAmount:0.##} EGP is required to use this code.");
        }

        var alreadyUsed = await _db.PromoCodeUsages.AsNoTracking()
            .AnyAsync(u => u.PromoCodeId == promo.Id && u.UserId == userId);

        if (alreadyUsed)
        {
            return Invalid("You have already used this promo code.");
        }

        // --- valid: compute the discount server-side ---
        var discountAmount = promo.DiscountType == DiscountType.Percentage
            ? Math.Round(subtotal * promo.DiscountValue / 100m, 2, MidpointRounding.AwayFromZero)
            : promo.DiscountValue;

        // Never discount below zero (a fixed discount larger than the subtotal).
        discountAmount = Math.Min(discountAmount, subtotal);

        return new PromoCodeValidationDto
        {
            IsValid = true,
            Message = promo.DiscountType == DiscountType.Percentage
                ? $"{promo.DiscountValue:0.##}% off applied."
                : $"{promo.DiscountValue:0.##} EGP off applied.",
            PromoCodeId = promo.Id,
            DiscountType = promo.DiscountType,
            DiscountValue = promo.DiscountValue,
            DiscountAmount = discountAmount
        };
    }

    public async Task<IList<PromoCodeDto>> GetPromoCodesAsync()
        => await _db.PromoCodes.AsNoTracking()
            .OrderByDescending(p => p.Id)
            .Select(p => new PromoCodeDto
            {
                Id = p.Id,
                Code = p.Code,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                MinimumOrderAmount = p.MinimumOrderAmount,
                ExpiryDate = p.ExpiryDate,
                UsageLimit = p.UsageLimit,
                TimesUsed = p.Usages.Count,
                IsActive = p.IsActive
            })
            .ToListAsync();

    public async Task<PromoCodeDto> CreateAsync(PromoCodeCreateDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();

        if (await _db.PromoCodes.AsNoTracking().AnyAsync(p => p.Code == code))
        {
            throw new AppException($"Promo code '{code}' already exists.", 409);
        }

        if (dto.DiscountType == DiscountType.Percentage && dto.DiscountValue > 100)
        {
            throw new AppException("A percentage discount cannot be greater than 100.", 400);
        }

        var promo = new PromoCode
        {
            Code = code,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MinimumOrderAmount = dto.MinimumOrderAmount,
            ExpiryDate = ToUtc(dto.ExpiryDate),
            UsageLimit = dto.UsageLimit,
            IsActive = true
        };

        _db.PromoCodes.Add(promo);
        await _db.SaveChangesAsync();

        return Map(promo, timesUsed: 0);
    }

    public async Task<PromoCodeDto?> UpdateAsync(int id, PromoCodeUpdateDto dto)
    {
        var promo = await _db.PromoCodes.FirstOrDefaultAsync(p => p.Id == id);

        if (promo is null)
        {
            return null;
        }

        if (dto.DiscountType == DiscountType.Percentage && dto.DiscountValue > 100)
        {
            throw new AppException("A percentage discount cannot be greater than 100.", 400);
        }

        promo.DiscountType = dto.DiscountType;
        promo.DiscountValue = dto.DiscountValue;
        promo.MinimumOrderAmount = dto.MinimumOrderAmount;
        promo.ExpiryDate = ToUtc(dto.ExpiryDate);
        promo.UsageLimit = dto.UsageLimit;
        promo.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();

        var timesUsed = await _db.PromoCodeUsages.AsNoTracking()
            .CountAsync(u => u.PromoCodeId == id);

        return Map(promo, timesUsed);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var promo = await _db.PromoCodes.FirstOrDefaultAsync(p => p.Id == id);

        if (promo is null)
        {
            return false;
        }

        promo.IsActive = false; // soft only - order history keeps its FK (Restrict)
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task RecordUsageAsync(int promoCodeId, string userId, int orderId)
    {
        _db.PromoCodeUsages.Add(new PromoCodeUsage
        {
            PromoCodeId = promoCodeId,
            UserId = userId,
            OrderId = orderId,
            UsedAt = DateTime.UtcNow
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Unique index (PromoCodeId, UserId): this customer already used the
            // code - the surrounding checkout transaction is rolled back.
            throw new AppException("You have already used this promo code.", 409);
        }
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static PromoCodeValidationDto Invalid(string message) => new()
    {
        IsValid = false,
        Message = message
    };

    /// <summary>
    /// Expiry dates are stored and compared in UTC (frontend contract: send
    /// dates with a 'Z' offset). A date with no offset at all is taken as UTC
    /// as-is, so "2026-12-31" does not get shifted by the server's local zone.
    /// </summary>
    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc) // Unspecified = already UTC
    };

    private static PromoCodeDto Map(PromoCode promo, int timesUsed) => new()
    {
        Id = promo.Id,
        Code = promo.Code,
        DiscountType = promo.DiscountType,
        DiscountValue = promo.DiscountValue,
        MinimumOrderAmount = promo.MinimumOrderAmount,
        ExpiryDate = promo.ExpiryDate,
        UsageLimit = promo.UsageLimit,
        TimesUsed = timesUsed,
        IsActive = promo.IsActive
    };
}
