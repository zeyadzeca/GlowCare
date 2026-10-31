using GlowCare.API.DTOs.Ingredients;

namespace GlowCare.API.Services.Interfaces;

/// <summary>Ingredient listing (public, used by filters) and management (Admin).</summary>
public interface IIngredientService
{
    /// <summary>All ingredients with their product counts (public).</summary>
    Task<IList<IngredientDto>> GetIngredientsAsync();

    Task<IngredientDto> CreateAsync(IngredientSaveDto dto);

    /// <summary>Returns null if the ingredient does not exist.</summary>
    Task<IngredientDto?> UpdateAsync(int id, IngredientSaveDto dto);

    /// <summary>Fails (returns false) while products still reference the ingredient.</summary>
    Task<bool> DeleteAsync(int id);
}
