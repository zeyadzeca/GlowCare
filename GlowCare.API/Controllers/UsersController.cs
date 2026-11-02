using GlowCare.API.DTOs.User;
using GlowCare.API.DTOs.Users;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// The AUTHENTICATED user's own profile, addresses and seller application.
/// The userId always comes from JWT claims - there is no route or body that
/// could point at another user's data.
/// </summary>
[ApiController]
[Authorize]
[Route("api/users/me")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    // ------------------------------------------------------------------
    // Profile
    // ------------------------------------------------------------------

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
        => Ok(await _users.GetProfileAsync(User.GetUserId()!));

    [HttpPut]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateProfileDto dto)
        => Ok(await _users.UpdateProfileAsync(User.GetUserId()!, dto));

    // ------------------------------------------------------------------
    // Addresses
    // ------------------------------------------------------------------

    [HttpGet("addresses")]
    public async Task<ActionResult<IList<AddressDto>>> GetAddresses()
        => Ok(await _users.GetAddressesAsync(User.GetUserId()!));

    [HttpPost("addresses")]
    public async Task<ActionResult<AddressDto>> AddAddress(AddressCreateDto dto)
        => StatusCode(StatusCodes.Status201Created, await _users.AddAddressAsync(User.GetUserId()!, dto));

    [HttpPut("addresses/{addressId:int}")]
    public async Task<ActionResult<AddressDto>> UpdateAddress(int addressId, AddressUpdateDto dto)
    {
        var address = await _users.UpdateAddressAsync(User.GetUserId()!, addressId, dto);

        return address is null
            ? NotFound(new { statusCode = 404, message = $"Address {addressId} was not found." })
            : Ok(address);
    }

    [HttpDelete("addresses/{addressId:int}")]
    public async Task<IActionResult> DeleteAddress(int addressId)
    {
        if (!await _users.DeleteAddressAsync(User.GetUserId()!, addressId))
        {
            return NotFound(new { statusCode = 404, message = $"Address {addressId} was not found." });
        }

        return Ok(new { message = "Address deleted." });
    }

    // ------------------------------------------------------------------
    // Phone Numbers
    // ------------------------------------------------------------------

    [HttpGet("phone-numbers")]
    public async Task<ActionResult<IList<UserPhoneNumberDto>>> GetPhoneNumbers()
        => Ok(await _users.GetPhoneNumbersAsync(User.GetUserId()!));

    [HttpPost("phone-numbers")]
    public async Task<ActionResult<UserPhoneNumberDto>> AddPhoneNumber(CreatePhoneNumberDto dto)
        => StatusCode(StatusCodes.Status201Created, await _users.AddPhoneNumberAsync(User.GetUserId()!, dto));

    [HttpPut("phone-numbers/{phoneId:int}")]
    public async Task<ActionResult<UserPhoneNumberDto>> UpdatePhoneNumber(int phoneId, UpdatePhoneNumberDto dto)
    {
        var phone = await _users.UpdatePhoneNumberAsync(User.GetUserId()!, phoneId, dto);

        return phone is null
            ? NotFound(new { statusCode = 404, message = $"Phone number {phoneId} was not found." })
            : Ok(phone);
    }

    [HttpDelete("phone-numbers/{phoneId:int}")]
    public async Task<IActionResult> DeletePhoneNumber(int phoneId)
    {
        if (!await _users.DeletePhoneNumberAsync(User.GetUserId()!, phoneId))
        {
            return NotFound(new { statusCode = 404, message = $"Phone number {phoneId} was not found." });
        }

        return Ok(new { message = "Phone number deleted." });
    }

    // ------------------------------------------------------------------
    // Seller application
    // ------------------------------------------------------------------

    /// <summary>Submits a seller application (creates a Pending profile).</summary>
    [HttpPost("seller-application")]
    public async Task<ActionResult<SellerProfileDto>> ApplyForSeller(SellerApplicationDto dto)
        => StatusCode(StatusCodes.Status201Created, await _users.ApplyForSellerAsync(User.GetUserId()!, dto));

    /// <summary>My seller profile (404 if I never applied).</summary>
    [HttpGet("seller-profile")]
    public async Task<ActionResult<SellerProfileDto>> GetSellerProfile()
    {
        var profile = await _users.GetSellerProfileAsync(User.GetUserId()!);

        return profile is null
            ? NotFound(new { statusCode = 404, message = "You have no seller profile." })
            : Ok(profile);
    }

    /// <summary>Update my seller profile (store name, description, logo).</summary>
    [HttpPut("seller-profile")]
    public async Task<ActionResult<SellerProfileDto>> UpdateSellerProfile(SellerApplicationDto dto)
    {
        var profile = await _users.UpdateSellerProfileAsync(User.GetUserId()!, dto);

        return profile is null
            ? NotFound(new { statusCode = 404, message = "You have no seller profile." })
            : Ok(profile);
    }

    /// <summary>Change my password.</summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var result = await _users.ChangePasswordAsync(User.GetUserId()!, dto);

        return result
            ? Ok(new { message = "Password changed successfully." })
            : BadRequest(new { statusCode = 400, message = "Current password is incorrect." });
    }
}
