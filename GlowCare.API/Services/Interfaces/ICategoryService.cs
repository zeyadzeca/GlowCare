using GlowCare.API.DTOs.Categories;

namespace GlowCare.API.Services.Interfaces;

/// <summary>Category listing (public) and management (Admin only).</summary>
public interface ICategoryService
{
    /// <summary>All categories with their active product counts (public).</summary>
    Task<IList<CategoryDto>> GetCategoriesAsync();

    /// <summary>Featured categories for homepage (sorted by product count, then name).</summary>
    Task<IList<CategoryDto>> GetFeaturedCategoriesAsync(int limit);

    Task<CategoryDto?> GetByIdAsync(int id);

    Task<CategoryDto> CreateAsync(CategoryCreateDto dto);

    /// <summary>Returns null if the category does not exist.</summary>
    Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto);

    /// <summary>Fails (returns false) while products still reference the category.</summary>
    Task<bool> DeleteAsync(int id);
}
