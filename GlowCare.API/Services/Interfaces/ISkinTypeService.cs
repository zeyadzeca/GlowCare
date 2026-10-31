using GlowCare.API.DTOs.SkinTypes;

namespace GlowCare.API.Services.Interfaces;

/// <summary>Skin type listing (public, used by filters and the home page) and management (Admin).</summary>
public interface ISkinTypeService
{
    /// <summary>All skin types (public).</summary>
    Task<IList<SkinTypeDto>> GetSkinTypesAsync();

    Task<SkinTypeDto> CreateAsync(SkinTypeSaveDto dto);

    /// <summary>Returns null if the skin type does not exist.</summary>
    Task<SkinTypeDto?> UpdateAsync(int id, SkinTypeSaveDto dto);

    /// <summary>Fails (returns false) while products still reference the skin type.</summary>
    Task<bool> DeleteAsync(int id);
}
