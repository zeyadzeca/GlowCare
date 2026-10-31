using GlowCare.API.DTOs.Categories;
using GlowCare.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlowCare.API.Controllers;

/// <summary>Category listing (public) and management (Admin only).</summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories)
    {
        _categories = categories;
    }

    /// <summary>All categories with their active product counts.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IList<CategoryDto>>> GetCategories()
        => Ok(await _categories.GetCategoriesAsync());

    /// <summary>One category by id.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _categories.GetByIdAsync(id);

        return category is null
            ? NotFound(new { statusCode = 404, message = $"Category {id} was not found." })
            : Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryCreateDto dto)
    {
        var created = await _categories.CreateAsync(dto);
        return CreatedAtAction(nameof(GetCategory), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, CategoryUpdateDto dto)
    {
        var updated = await _categories.UpdateAsync(id, dto);

        return updated is null
            ? NotFound(new { statusCode = 404, message = $"Category {id} was not found." })
            : Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        if (!await _categories.DeleteAsync(id))
        {
            // Either it does not exist, or products still reference it.
            var exists = await _categories.GetByIdAsync(id);

            return exists is null
                ? NotFound(new { statusCode = 404, message = $"Category {id} was not found." })
                : Conflict(new
                {
                    statusCode = 409,
                    message = "This category cannot be deleted because products still reference it."
                });
        }

        return Ok(new { message = "Category deleted." });
    }
}
