using GlowCare.API.DTOs.Brands;

namespace GlowCare.API.Services.Interfaces;

/// <summary>Brand listing (public) and management (Admin only).</summary>
public interface IBrandService
{
    /// <summary>All brands with their active product counts (public).</summary>
    Task<IList<BrandDto>> GetBrandsAsync();

    /// <summary>Featured brands for homepage (sorted by product count, then name).</summary>
    Task<IList<BrandDto>> GetFeaturedBrandsAsync(int limit);

    Task<BrandDto?> GetByIdAsync(int id);

    Task<BrandDto> CreateAsync(BrandCreateDto dto);

    /// <summary>Returns null if the brand does not exist.</summary>
    Task<BrandDto?> UpdateAsync(int id, BrandUpdateDto dto);

    /// <summary>Fails (returns false) while products still reference the brand.</summary>
    Task<bool> DeleteAsync(int id);
}
