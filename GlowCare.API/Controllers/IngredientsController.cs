using GlowCare.API.DTOs.Ingredients;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>Ingredient listing (public: product filters) and management (Admin only).</summary>
[ApiController]
[Route("api/ingredients")]
public class IngredientsController : ControllerBase
{
    private readonly IIngredientService _ingredients;

    public IngredientsController(IIngredientService ingredients)
    {
        _ingredients = ingredients;
    }

    /// <summary>All ingredients with their product counts.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IList<IngredientDto>>> GetIngredients()
        => Ok(await _ingredients.GetIngredientsAsync());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<IngredientDto>> CreateIngredient(IngredientSaveDto dto)
    {
        var created = await _ingredients.CreateAsync(dto);
        return CreatedAtAction(nameof(GetIngredients), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<IngredientDto>> UpdateIngredient(int id, IngredientSaveDto dto)
    {
        var updated = await _ingredients.UpdateAsync(id, dto);

        return updated is null
            ? NotFound(new { statusCode = 404, message = $"Ingredient {id} was not found." })
            : Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteIngredient(int id)
    {
        if (!await _ingredients.DeleteAsync(id))
        {
            var stillExists = (await _ingredients.GetIngredientsAsync()).Any(i => i.Id == id);

            return stillExists
                ? Conflict(new
                {
                    statusCode = 409,
                    message = "This ingredient cannot be deleted because products still reference it."
                })
                : NotFound(new { statusCode = 404, message = $"Ingredient {id} was not found." });
        }

        return Ok(new { message = "Ingredient deleted." });
    }
}
