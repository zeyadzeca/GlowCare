using GlowCare.API.DTOs.Promotions;
using GlowCare.API.Helpers;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>
/// Promo codes: validation is for the customer's checkout; everything else is
/// Admin management. The discount amount is always computed by the backend.
/// </summary>
[ApiController]
[Route("api/promocodes")]
public class PromoCodesController : ControllerBase
{
    private readonly IPromoCodeService _promoCodes;

    public PromoCodesController(IPromoCodeService promoCodes)
    {
        _promoCodes = promoCodes;
    }

    /// <summary>
    /// Checks a code against the given cart subtotal: exists, active, not
    /// expired, limit not reached, minimum met, not already used by this
    /// customer. Returns the computed discount amount (or the reason it fails).
    /// </summary>
    [Authorize(Roles = "Customer")]
    [HttpPost("validate")]
    public async Task<ActionResult<PromoCodeValidationDto>> ValidatePromoCode(ValidatePromoCodeDto dto)
        => Ok(await _promoCodes.ValidatePromoCodeAsync(User.GetUserId()!, dto));

    /// <summary>All promo codes with usage counts.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IList<PromoCodeDto>>> GetPromoCodes()
        => Ok(await _promoCodes.GetPromoCodesAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<PromoCodeDto>> CreatePromoCode(PromoCodeCreateDto dto)
    {
        var created = await _promoCodes.CreateAsync(dto);
        return CreatedAtAction(nameof(GetPromoCodes), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PromoCodeDto>> UpdatePromoCode(int id, PromoCodeUpdateDto dto)
    {
        var updated = await _promoCodes.UpdateAsync(id, dto);

        return updated is null
            ? NotFound(new { statusCode = 404, message = $"Promo code {id} was not found." })
            : Ok(updated);
    }

    /// <summary>Deactivates a code (soft delete - order history keeps its FK).</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivatePromoCode(int id)
    {
        if (!await _promoCodes.DeactivateAsync(id))
        {
            return NotFound(new { statusCode = 404, message = $"Promo code {id} was not found." });
        }

        return Ok(new { message = "Promo code deactivated." });
    }
}
