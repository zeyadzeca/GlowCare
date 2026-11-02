using GlowCare.API.Data;
using GlowCare.API.DTOs.User;
using GlowCare.API.DTOs.Users;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Identity;
using GlowCare.API.Models.User;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Profile management, address management and the seller application flow.
///
/// OWNERSHIP RULE: every query is filtered by the userId that the controller
/// reads from JWT claims. A user can never see or modify another user's data,
/// even by guessing ids - unowned rows simply are not found (null/false).
/// </summary>
public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ------------------------------------------------------------------
    // Profile
    // ------------------------------------------------------------------

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new AppException("User not found.", 404);

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList()
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new AppException("User not found.", 404);

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new AppException(
                string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return await GetProfileAsync(userId);
    }

    // ------------------------------------------------------------------
    // Addresses
    // ------------------------------------------------------------------

    public async Task<IList<AddressDto>> GetAddressesAsync(string userId)
    {
        return await _db.Addresses
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Id)
            .Select(a => ToDto(a))
            .ToListAsync();
    }

    public async Task<AddressDto> AddAddressAsync(string userId, AddressCreateDto dto)
    {
        var (city, street, building) = NormalizeAddress(dto.City, dto.Street, dto.BuildingNumber);

        var existing = await _db.Addresses
            .FirstOrDefaultAsync(a =>
                a.UserId == userId
                && a.City == city
                && a.Street == street
                && a.BuildingNumber == building);

        if (existing is not null)
        {
            return ToDto(existing);
        }

        var address = new Address
        {
            UserId = userId,
            City = city,
            Street = street,
            BuildingNumber = building
        };

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();

        return ToDto(address);
    }

    public async Task<AddressDto?> UpdateAddressAsync(string userId, int addressId, AddressUpdateDto dto)
    {
        var (city, street, building) = NormalizeAddress(dto.City, dto.Street, dto.BuildingNumber);

        var address = await _db.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        if (address is null)
        {
            return null; // not found or not owned by this user
        }

        address.City = city;
        address.Street = street;
        address.BuildingNumber = building;

        await _db.SaveChangesAsync();

        return ToDto(address);
    }

    public async Task<bool> DeleteAddressAsync(string userId, int addressId)
    {
        var address = await _db.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        if (address is null)
        {
            return false; // not found or not owned by this user
        }

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();

        return true;
    }

    // ------------------------------------------------------------------
    // Phone Numbers
    // ------------------------------------------------------------------

    public async Task<IList<UserPhoneNumberDto>> GetPhoneNumbersAsync(string userId)
    {
        return await _db.UserPhoneNumbers
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.IsDefault ? 0 : 1)
            .ThenBy(p => p.Id)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<UserPhoneNumberDto> AddPhoneNumberAsync(string userId, CreatePhoneNumberDto dto)
    {
        var normalizedPhone = NormalizePhoneNumber(dto.PhoneNumber);

        var existing = await FindPhoneNumberAsync(userId, normalizedPhone);

        if (existing is not null)
        {
            // Re-saving the same number is treated as "already saved".
            return ToDto(existing);
        }

        // A customer can only have ONE primary phone number. Adding another one
        // is rejected so the client can switch to editing the existing number.
        if (await _db.UserPhoneNumbers.AnyAsync(p => p.UserId == userId))
        {
            throw new AppException("You already have a phone number saved. Edit it instead of adding another one.", 409);
        }

        var phoneNumber = new UserPhoneNumber
        {
            UserId = userId,
            PhoneNumber = normalizedPhone,
            IsDefault = true, // the single phone number is always the primary one
            CreatedAt = DateTime.UtcNow
        };

        _db.UserPhoneNumbers.Add(phoneNumber);
        await _db.SaveChangesAsync();

        return ToDto(phoneNumber);
    }

    public async Task<UserPhoneNumberDto?> UpdatePhoneNumberAsync(string userId, int phoneId, UpdatePhoneNumberDto dto)
    {
        var normalizedPhone = NormalizePhoneNumber(dto.PhoneNumber);

        var phoneNumber = await _db.UserPhoneNumbers
            .FirstOrDefaultAsync(p => p.Id == phoneId && p.UserId == userId);

        if (phoneNumber is null)
        {
            return null; // not found or not owned by this user
        }

        // An update may never turn this row into a duplicate of ANOTHER saved number.
        var duplicate = await FindPhoneNumberAsync(userId, normalizedPhone);
        if (duplicate is not null && duplicate.Id != phoneId)
        {
            throw new AppException("You already have this phone number saved.", 409);
        }

        phoneNumber.PhoneNumber = normalizedPhone;
        // The single saved number is always the primary one.
        phoneNumber.IsDefault = true;

        await _db.SaveChangesAsync();

        return ToDto(phoneNumber);
    }

    public async Task<bool> DeletePhoneNumberAsync(string userId, int phoneId)
    {
        var phoneNumber = await _db.UserPhoneNumbers
            .FirstOrDefaultAsync(p => p.Id == phoneId && p.UserId == userId);

        if (phoneNumber is null)
        {
            return false; // not found or not owned by this user
        }

        // If deleting the default, the oldest remaining number becomes the default.
        if (phoneNumber.IsDefault)
        {
            var nextDefault = await _db.UserPhoneNumbers
                .Where(p => p.UserId == userId && p.Id != phoneId)
                .OrderBy(p => p.Id)
                .FirstOrDefaultAsync();

            if (nextDefault is not null)
            {
                nextDefault.IsDefault = true;
            }
        }

        _db.UserPhoneNumbers.Remove(phoneNumber);
        await _db.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Trims and collapses inner whitespace, then rejects empty values the same
    /// way NormalizeAddress does (required, max 20 characters).
    /// </summary>
    private static string NormalizePhoneNumber(string? phoneNumber)
    {
        var value = string.Join(
            " ",
            (phoneNumber ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        if (string.IsNullOrEmpty(value))
        {
            throw new AppException("Phone number is required.", 400);
        }

        if (value.Length > 20)
        {
            throw new AppException("Phone number must be 20 characters or fewer.", 400);
        }

        return value;
    }

    /// <summary>
    /// Finds a saved number of this user whose digits match the given value, so
    /// "01001112233" and "0100 111 2233" count as the same number. Only the
    /// caller's own rows are ever scanned.
    /// </summary>
    private async Task<UserPhoneNumber?> FindPhoneNumberAsync(string userId, string phoneNumber)
    {
        var key = PhoneSortKey(phoneNumber);
        var saved = await _db.UserPhoneNumbers
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return saved.FirstOrDefault(p => PhoneSortKey(p.PhoneNumber) == key);
    }

    private static string PhoneSortKey(string phoneNumber)
        => new string(phoneNumber.Where(char.IsDigit).ToArray());

    // ------------------------------------------------------------------
    // Seller application
    // ------------------------------------------------------------------

    public async Task<SellerProfileDto> ApplyForSellerAsync(string userId, SellerApplicationDto dto)
    {
        var alreadyApplied = await _db.SellerProfiles
            .AnyAsync(s => s.UserId == userId);

        if (alreadyApplied)
        {
            throw new AppException("You have already applied to become a seller.");
        }

        var profile = new SellerProfile
        {
            UserId = userId,
            StoreName = dto.StoreName,
            Description = dto.Description,
            Logo = dto.Logo,
            ApprovalStatus = Enums.ApprovalStatus.Pending // admin must approve
        };

        _db.SellerProfiles.Add(profile);
        await _db.SaveChangesAsync();

        // Grant the Seller role so the user can reach the seller endpoints.
        // Publishing products is still blocked until ApprovalStatus = Approved.
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new AppException("User not found.", 404);

        if (!await _userManager.IsInRoleAsync(user, "Seller"))
        {
            await _userManager.AddToRoleAsync(user, "Seller");
        }

        return ToDto(profile);
    }

    public async Task<SellerProfileDto?> GetSellerProfileAsync(string userId)
    {
        var profile = await _db.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        return profile is null ? null : ToDto(profile);
    }

    public async Task<SellerProfileDto?> UpdateSellerProfileAsync(string userId, SellerApplicationDto dto)
    {
        var profile = await _db.SellerProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (profile is null) return null;

        profile.StoreName = dto.StoreName.Trim();
        profile.Description = dto.Description?.Trim() ?? string.Empty;
        profile.Logo = dto.Logo?.Trim() ?? string.Empty;

        await _db.SaveChangesAsync();
        return ToDto(profile);
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        return result.Succeeded;
    }

    // ------------------------------------------------------------------
    // Mapping helpers (entities never leave the service layer)
    // ------------------------------------------------------------------

    private static (string City, string Street, string Building) NormalizeAddress(
        string city, string street, string buildingNumber)
    {
        var cityValue = city.Trim();
        var streetValue = street.Trim();
        var buildingValue = buildingNumber.Trim();

        if (string.IsNullOrEmpty(cityValue) || string.IsNullOrEmpty(streetValue) || string.IsNullOrEmpty(buildingValue))
        {
            throw new AppException("City, street, and building number are required.", 400);
        }

        return (cityValue, streetValue, buildingValue);
    }

    private static AddressDto ToDto(Address a) => new()
    {
        Id = a.Id,
        City = a.City,
        Street = a.Street,
        BuildingNumber = a.BuildingNumber
    };

    private static UserPhoneNumberDto ToDto(UserPhoneNumber p) => new()
    {
        Id = p.Id,
        PhoneNumber = p.PhoneNumber,
        IsDefault = p.IsDefault,
        CreatedAt = p.CreatedAt
    };

    private static SellerProfileDto ToDto(SellerProfile s) => new()
    {
        Id = s.Id,
        StoreName = s.StoreName,
        Description = s.Description,
        Logo = s.Logo,
        ApprovalStatus = s.ApprovalStatus,
        CreatedAt = s.CreatedAt
    };
}
