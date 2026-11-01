using GlowCare.API.Data;
using GlowCare.API.DTOs.Brands;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Catalog;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Brand listing (public) and management (Admin only - enforced by the
/// controller). Deletion is blocked while products still reference the brand.
/// </summary>
public class BrandService : IBrandService
{
    private readonly AppDbContext _db;

    public BrandService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IList<BrandDto>> GetBrandsAsync()
        => await _db.Brands.AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Logo = b.Logo,
                ProductCount = b.Products.Count(p => p.IsActive)
            })
            .ToListAsync();

    public async Task<IList<BrandDto>> GetFeaturedBrandsAsync(int limit)
        => await _db.Brands.AsNoTracking()
            .Select(b => new
            {
                Brand = b,
                ProductCount = b.Products.Count(p => p.IsActive)
            })
            .Where(x => x.ProductCount > 0)
            .OrderByDescending(x => x.ProductCount)
            .ThenBy(x => x.Brand.Name)
            .Select(x => new BrandDto
            {
                Id = x.Brand.Id,
                Name = x.Brand.Name,
                Description = x.Brand.Description,
                Logo = x.Brand.Logo,
                ProductCount = x.ProductCount
            })
            .Take(limit)
            .ToListAsync();

    public async Task<BrandDto?> GetByIdAsync(int id)
        => await _db.Brands.AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Logo = b.Logo,
                ProductCount = b.Products.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync();

    public async Task<BrandDto> CreateAsync(BrandCreateDto dto)
    {
        var name = dto.Name.Trim();
        await EnsureNameIsFreeAsync(name);

        var brand = new Brand
        {
            Name = name,
            Description = dto.Description.Trim(),
            Logo = dto.Logo.Trim()
        };

        _db.Brands.Add(brand);
        await _db.SaveChangesAsync();

        return Map(brand, productCount: 0);
    }

    public async Task<BrandDto?> UpdateAsync(int id, BrandUpdateDto dto)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id);

        if (brand is null)
        {
            return null;
        }

        var name = dto.Name.Trim();
        await EnsureNameIsFreeAsync(name, excludeId: id);

        brand.Name = name;
        brand.Description = dto.Description.Trim();
        brand.Logo = dto.Logo.Trim();
        await _db.SaveChangesAsync();

        return Map(brand, await _db.Products.CountAsync(p => p.BrandId == id && p.IsActive));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var brand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id);

        if (brand is null)
        {
            return false;
        }

        _db.Brands.Remove(brand);

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false; // products still reference this brand
        }
    }

    // ------------------------------------------------------------------

    private async Task EnsureNameIsFreeAsync(string name, int? excludeId = null)
    {
        var exists = await _db.Brands.AsNoTracking()
            .AnyAsync(b => b.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || b.Id != excludeId.Value));

        if (exists)
        {
            throw new AppException($"A brand named '{name}' already exists.", 409);
        }
    }

    private static BrandDto Map(Brand brand, int productCount) => new()
    {
        Id = brand.Id,
        Name = brand.Name,
        Description = brand.Description,
        Logo = brand.Logo,
        ProductCount = productCount
    };
}
