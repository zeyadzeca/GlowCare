using GlowCare.API.Data;
using GlowCare.API.DTOs.Ingredients;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Catalog;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Ingredient listing (public: product filters) and management (Admin only).
/// </summary>
public class IngredientService : IIngredientService
{
    private readonly AppDbContext _db;

    public IngredientService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IList<IngredientDto>> GetIngredientsAsync()
        => await _db.Ingredients.AsNoTracking()
            .OrderBy(i => i.Name)
            .Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                ProductCount = i.ProductIngredients.Count(pi => pi.Product.IsActive)
            })
            .ToListAsync();

    public async Task<IngredientDto> CreateAsync(IngredientSaveDto dto)
    {
        var name = dto.Name.Trim();
        await EnsureNameIsFreeAsync(name);

        var ingredient = new Ingredient
        {
            Name = name,
            Description = dto.Description.Trim()
        };

        _db.Ingredients.Add(ingredient);
        await _db.SaveChangesAsync();

        return Map(ingredient, productCount: 0);
    }

    public async Task<IngredientDto?> UpdateAsync(int id, IngredientSaveDto dto)
    {
        var ingredient = await _db.Ingredients.FirstOrDefaultAsync(i => i.Id == id);

        if (ingredient is null)
        {
            return null;
        }

        var name = dto.Name.Trim();
        await EnsureNameIsFreeAsync(name, excludeId: id);

        ingredient.Name = name;
        ingredient.Description = dto.Description.Trim();
        await _db.SaveChangesAsync();

        var count = await _db.ProductIngredients.AsNoTracking()
            .CountAsync(pi => pi.IngredientId == id && pi.Product.IsActive);

        return Map(ingredient, count);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ingredient = await _db.Ingredients.FirstOrDefaultAsync(i => i.Id == id);

        if (ingredient is null)
        {
            return false;
        }

        _db.Ingredients.Remove(ingredient);

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false; // products still reference this ingredient
        }
    }

    // ------------------------------------------------------------------

    private async Task EnsureNameIsFreeAsync(string name, int? excludeId = null)
    {
        var exists = await _db.Ingredients.AsNoTracking()
            .AnyAsync(i => i.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || i.Id != excludeId.Value));

        if (exists)
        {
            throw new AppException($"An ingredient named '{name}' already exists.", 409);
        }
    }

    private static IngredientDto Map(Ingredient ingredient, int productCount) => new()
    {
        Id = ingredient.Id,
        Name = ingredient.Name,
        Description = ingredient.Description,
        ProductCount = productCount
    };
}
