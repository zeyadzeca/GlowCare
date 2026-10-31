using GlowCare.API.DTOs.SkinTypes;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>Skin type listing (public: filters + home page) and management (Admin only).</summary>
[ApiController]
[Route("api/skintypes")]
public class SkinTypesController : ControllerBase
{
    private readonly ISkinTypeService _skinTypes;

    public SkinTypesController(ISkinTypeService skinTypes)
    {
        _skinTypes = skinTypes;
    }

    /// <summary>All skin types (Id + Name).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IList<SkinTypeDto>>> GetSkinTypes()
        => Ok(await _skinTypes.GetSkinTypesAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<SkinTypeDto>> CreateSkinType(SkinTypeSaveDto dto)
    {
        var created = await _skinTypes.CreateAsync(dto);
        return CreatedAtAction(nameof(GetSkinTypes), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SkinTypeDto>> UpdateSkinType(int id, SkinTypeSaveDto dto)
    {
        var updated = await _skinTypes.UpdateAsync(id, dto);

        return updated is null
            ? NotFound(new { statusCode = 404, message = $"Skin type {id} was not found." })
            : Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSkinType(int id)
    {
        if (!await _skinTypes.DeleteAsync(id))
        {
            // Not found vs "still used by products" - check which one happened.
            var stillExists = (await _skinTypes.GetSkinTypesAsync()).Any(s => s.Id == id);

            return stillExists
                ? Conflict(new
                {
                    statusCode = 409,
                    message = "This skin type cannot be deleted because products still reference it."
                })
                : NotFound(new { statusCode = 404, message = $"Skin type {id} was not found." });
        }

        return Ok(new { message = "Skin type deleted." });
    }
}
