using GlowCare.API.DTOs.Banners;

namespace GlowCare.API.Services.Interfaces;

/// <summary>
/// Home-page banners: active banners are public, full management is Admin only.
/// </summary>
public interface IBannerService
{
    /// <summary>Only IsActive = true banners (public, home page).</summary>
    Task<IList<BannerDto>> GetActiveBannersAsync();

    /// <summary>All banners including inactive (Admin only).</summary>
    Task<IList<BannerDto>> GetAllBannersAsync();

    Task<BannerDto> CreateAsync(BannerSaveDto dto);

    /// <summary>Returns null if the banner does not exist.</summary>
    Task<BannerDto?> UpdateAsync(int id, BannerSaveDto dto);

    /// <summary>Deletes a banner. Returns false if not found.</summary>
    Task<bool> DeleteAsync(int id);
}
