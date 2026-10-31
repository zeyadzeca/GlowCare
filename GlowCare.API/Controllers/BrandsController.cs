using GlowCare.API.DTOs.Brands;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>Brand listing (public) and management (Admin only).</summary>
[ApiController]
[Route("api/brands")]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brands;

    public BrandsController(IBrandService brands)
    {
        _brands = brands;
    }

    /// <summary>All brands with their active product counts.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IList<BrandDto>>> GetBrands()
        => Ok(await _brands.GetBrandsAsync());

    /// <summary>One brand by id.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<BrandDto>> GetBrand(int id)
    {
        var brand = await _brands.GetByIdAsync(id);

        return brand is null
            ? NotFound(new { statusCode = 404, message = $"Brand {id} was not found." })
            : Ok(brand);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<BrandDto>> CreateBrand(BrandCreateDto dto)
    {
        var created = await _brands.CreateAsync(dto);
        return CreatedAtAction(nameof(GetBrand), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BrandDto>> UpdateBrand(int id, BrandUpdateDto dto)
    {
        var updated = await _brands.UpdateAsync(id, dto);

        return updated is null
            ? NotFound(new { statusCode = 404, message = $"Brand {id} was not found." })
            : Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        if (!await _brands.DeleteAsync(id))
        {
            var exists = await _brands.GetByIdAsync(id);

            return exists is null
                ? NotFound(new { statusCode = 404, message = $"Brand {id} was not found." })
                : Conflict(new
                {
                    statusCode = 409,
                    message = "This brand cannot be deleted because products still reference it."
                });
        }

        return Ok(new { message = "Brand deleted." });
    }
}
