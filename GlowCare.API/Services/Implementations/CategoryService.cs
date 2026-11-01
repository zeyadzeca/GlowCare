using GlowCare.API.Data;
using GlowCare.API.DTOs.Categories;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Catalog;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Category listing (public) and management (Admin only - enforced by the
/// controller). Deletion is blocked while products still reference the
/// category (Restrict FK); we surface that as a clean 409 via the bool result.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IList<CategoryDto>> GetCategoriesAsync()
        => await _db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ProductCount = c.Products.Count(p => p.IsActive) // only active products count
            })
            .ToListAsync();

    public async Task<IList<CategoryDto>> GetFeaturedCategoriesAsync(int limit)
        => await _db.Categories.AsNoTracking()
            .Select(c => new
            {
                Category = c,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .Where(x => x.ProductCount > 0)
            .OrderByDescending(x => x.ProductCount)
            .ThenBy(x => x.Category.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Category.Id,
                Name = x.Category.Name,
                Description = x.Category.Description,
                ImageUrl = x.Category.ImageUrl,
                ProductCount = x.ProductCount
            })
            .Take(limit)
            .ToListAsync();

    public async Task<CategoryDto?> GetByIdAsync(int id)
        => await _db.Categories.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync();

    public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
    {
        var name = dto.Name.Trim();
        await EnsureNameIsFreeAsync(name);

        var category = new Category
        {
            Name = name,
            Description = dto.Description.Trim(),
            ImageUrl = dto.ImageUrl.Trim()
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Map(category, productCount: 0);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            return null;
        }

        var name = dto.Name.Trim();
        await EnsureNameIsFreeAsync(name, excludeId: id);

        category.Name = name;
        category.Description = dto.Description.Trim();
        category.ImageUrl = dto.ImageUrl.Trim();
        await _db.SaveChangesAsync();

        return Map(category, await _db.Products.CountAsync(p => p.CategoryId == id && p.IsActive));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            return false;
        }

        _db.Categories.Remove(category);

        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false; // products still reference this category
        }
    }

    // ------------------------------------------------------------------

    private async Task EnsureNameIsFreeAsync(string name, int? excludeId = null)
    {
        var exists = await _db.Categories.AsNoTracking()
            .AnyAsync(c => c.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
        {
            throw new AppException($"A category named '{name}' already exists.", 409);
        }
    }

    private static CategoryDto Map(Category category, int productCount) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        ImageUrl = category.ImageUrl,
        ProductCount = productCount
    };
}
