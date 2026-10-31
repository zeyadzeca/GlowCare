using GlowCare.API.Data;
using GlowCare.API.DTOs.Banners;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Content;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>Home-page banners: active ones are public, management is Admin only.</summary>
public class BannerService : IBannerService
{
    private readonly AppDbContext _db;

    public BannerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IList<BannerDto>> GetActiveBannersAsync()
        => await _db.Banners.AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Id)
            .Select(b => Map(b))
            .ToListAsync();

    public async Task<IList<BannerDto>> GetAllBannersAsync()
        => await _db.Banners.AsNoTracking()
            .OrderBy(b => b.Id)
            .Select(b => Map(b))
            .ToListAsync();

    public async Task<BannerDto> CreateAsync(BannerSaveDto dto)
    {
        var banner = new Banner
        {
            Title = dto.Title.Trim(),
            ImageUrl = dto.ImageUrl.Trim(),
            Link = dto.Link.Trim(),
            IsActive = dto.IsActive
        };

        _db.Banners.Add(banner);
        await _db.SaveChangesAsync();

        return Map(banner);
    }

    public async Task<BannerDto?> UpdateAsync(int id, BannerSaveDto dto)
    {
        var banner = await _db.Banners.FirstOrDefaultAsync(b => b.Id == id);

        if (banner is null)
        {
            return null;
        }

        banner.Title = dto.Title.Trim();
        banner.ImageUrl = dto.ImageUrl.Trim();
        banner.Link = dto.Link.Trim();
        banner.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();

        return Map(banner);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var banner = await _db.Banners.FirstOrDefaultAsync(b => b.Id == id);

        if (banner is null)
        {
            return false;
        }

        _db.Banners.Remove(banner);
        await _db.SaveChangesAsync();
        return true;
    }

    private static BannerDto Map(Banner banner) => new()
    {
        Id = banner.Id,
        Title = banner.Title,
        ImageUrl = banner.ImageUrl,
        Link = banner.Link,
        IsActive = banner.IsActive
    };
}
