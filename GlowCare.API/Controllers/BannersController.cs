using GlowCare.API.DTOs.Banners;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>Home-page banners: active ones are public, management is Admin only.</summary>
[ApiController]
[Route("api/banners")]
public class BannersController : ControllerBase
{
    private readonly IBannerService _banners;

    public BannersController(IBannerService banners)
    {
        _banners = banners;
    }

    /// <summary>Only active banners (public, home page carousel).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IList<BannerDto>>> GetActiveBanners()
        => Ok(await _banners.GetActiveBannersAsync());

    /// <summary>All banners including inactive ones (Admin).</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<ActionResult<IList<BannerDto>>> GetAllBanners()
        => Ok(await _banners.GetAllBannersAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<BannerDto>> CreateBanner(BannerSaveDto dto)
    {
        var created = await _banners.CreateAsync(dto);
        return CreatedAtAction(nameof(GetActiveBanners), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BannerDto>> UpdateBanner(int id, BannerSaveDto dto)
    {
        var updated = await _banners.UpdateAsync(id, dto);

        return updated is null
            ? NotFound(new { statusCode = 404, message = $"Banner {id} was not found." })
            : Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBanner(int id)
    {
        if (!await _banners.DeleteAsync(id))
        {
            return NotFound(new { statusCode = 404, message = $"Banner {id} was not found." });
        }

        return Ok(new { message = "Banner deleted." });
    }
}
