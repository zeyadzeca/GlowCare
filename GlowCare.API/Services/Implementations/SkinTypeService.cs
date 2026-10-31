using GlowCare.API.Data;
using GlowCare.API.DTOs.SkinTypes;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Catalog;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Skin type listing (public: powers filters + home page cards) and
/// management (Admin only). Schema is Id + Name only - no description.
/// </summary>
public class SkinTypeService : ISkinTypeService
{
    private readonly AppDbContext _db;

    public SkinTypeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IList<SkinTypeDto>> GetSkinTypesAsync()
        => await _db.SkinTypes.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SkinTypeDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync();

    public async Task<SkinTypeDto> CreateAsync(SkinTypeSaveDto dto)
    {
        var name = dto.Name.Trim();
        await EnsureNameIsFreeAsync(name);

        var skinType = new SkinType { Name = name };
        _db.SkinTypes.Add(skinType);
        await _db.SaveChangesAsync();

        return new SkinTypeDto { Id = skinType.Id, Name = skinType.Name };
    }

    public async Task<SkinTypeDto?> UpdateAsync(int id, SkinTypeSaveDto dto)
    {
        var skinType = await _db.SkinTypes.FirstOrDefaultAsync(s => s.Id == id);

        if (skinType is null)
        {
            return null;
        }

        var name = dto.Name.Trim();
        await EnsureNameIsFreeAsync(name, excludeId: id);

        skinType.Name = name;
        await _db.SaveChangesAsync();

        return new SkinTypeDto { Id = skinType.Id, Name = skinType.Name };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var skinType = await _db.SkinTypes.FirstOrDefaultAsync(s => s.Id == id);

        if (skinType is null)
        {
            return false;
        }

        _db.SkinTypes.Remove(skinType);

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false; // products still reference this skin type
        }
    }

    // ------------------------------------------------------------------

    private async Task EnsureNameIsFreeAsync(string name, int? excludeId = null)
    {
        var exists = await _db.SkinTypes.AsNoTracking()
            .AnyAsync(s => s.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || s.Id != excludeId.Value));

        if (exists)
        {
            throw new AppException($"A skin type named '{name}' already exists.", 409);
        }
    }
}
