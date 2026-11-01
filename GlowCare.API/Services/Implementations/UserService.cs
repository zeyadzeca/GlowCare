using GlowCare.API.Data;
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
