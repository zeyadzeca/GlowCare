using GlowCare.API.DTOs.Users;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// Profile management, address management and the seller application flow.
/// All methods operate on the AUTHENTICATED user only - the userId always
/// comes from JWT claims, never from the request body.
/// </summary>
public interface IUserService
{
    /// <summary>Gets the profile of the authenticated user.</summary>
    Task<UserProfileDto> GetProfileAsync(string userId);

    /// <summary>Updates the profile (full name, phone) of the authenticated user.</summary>
    Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);

    /// <summary>Lists all addresses of the authenticated user.</summary>
    Task<IList<AddressDto>> GetAddressesAsync(string userId);

    /// <summary>Adds a new address for the authenticated user.</summary>
    Task<AddressDto> AddAddressAsync(string userId, AddressCreateDto dto);

    /// <summary>Updates one of the user's own addresses. Returns false if not found/not owned.</summary>
    Task<AddressDto?> UpdateAddressAsync(string userId, int addressId, AddressUpdateDto dto);

    /// <summary>Deletes one of the user's own addresses. Returns false if not found/not owned.</summary>
    Task<bool> DeleteAddressAsync(string userId, int addressId);

    /// <summary>
    /// Submits a seller application: creates a SellerProfile with
    /// ApprovalStatus = Pending and assigns the Seller role.
    /// </summary>
    Task<SellerProfileDto> ApplyForSellerAsync(string userId, SellerApplicationDto dto);

    /// <summary>Gets the seller profile of the authenticated user (null if none).</summary>
    Task<SellerProfileDto?> GetSellerProfileAsync(string userId);
}
