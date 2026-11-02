using GlowCare.API.DTOs.User;
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

    /// <summary>Lists all phone numbers of the authenticated user.</summary>
    Task<IList<UserPhoneNumberDto>> GetPhoneNumbersAsync(string userId);

    /// <summary>Adds a new phone number for the authenticated user.</summary>
    Task<UserPhoneNumberDto> AddPhoneNumberAsync(string userId, CreatePhoneNumberDto dto);

    /// <summary>Updates one of the user's own phone numbers. Returns false if not found/not owned.</summary>
    Task<UserPhoneNumberDto?> UpdatePhoneNumberAsync(string userId, int phoneId, UpdatePhoneNumberDto dto);

    /// <summary>Deletes one of the user's own phone numbers. Returns false if not found/not owned.</summary>
    Task<bool> DeletePhoneNumberAsync(string userId, int phoneId);

    /// <summary>
    /// Submits a seller application: creates a SellerProfile with
    /// ApprovalStatus = Pending and assigns the Seller role.
    /// </summary>
    Task<SellerProfileDto> ApplyForSellerAsync(string userId, SellerApplicationDto dto);

    /// <summary>Gets the seller profile of the authenticated user (null if none).</summary>
    Task<SellerProfileDto?> GetSellerProfileAsync(string userId);

    /// <summary>Updates the seller profile (store name, description, logo) of the authenticated user.</summary>
    Task<SellerProfileDto?> UpdateSellerProfileAsync(string userId, SellerApplicationDto dto);

    /// <summary>Changes the user's password. Returns true if successful, false if current password is wrong.</summary>
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto);
}
