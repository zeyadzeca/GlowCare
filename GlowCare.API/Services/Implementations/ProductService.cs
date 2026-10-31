using GlowCare.API.DTOs.Common;
using GlowCare.API.Data;
using GlowCare.API.DTOs.Products;
using GlowCare.API.Enums;
using GlowCare.API.Helpers;
using GlowCare.API.Models.Catalog;
using GlowCare.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlowCare.API.Services.Implementations;

/// <summary>
/// Public product catalog + seller product management.
///
/// RULES enforced here:
/// - Only IsActive = true products are listed publicly.
/// - Only an APPROVED seller may publish (checked against SellerProfile).
/// - A seller can only edit/deactivate products they own (sellerId comes from
///   JWT claims, never from the request).
/// - Brand/category/skin-type/ingredient ids are validated before touching
///   the database (FK violations become clean 400 errors instead of 500s).
/// </summary>
public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    // ------------------------------------------------------------------
    // Public catalog
    // ------------------------------------------------------------------

    public async Task<PagedResultDto<ProductListDto>> GetProductsAsync(ProductFilterDto filter)
    {
        var query = _db.Products.AsNoTracking().Where(p => p.IsActive);

        // --- optional filters (all combined with AND) ---
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(p => p.Name.Contains(term));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (filter.BrandId.HasValue)
        {
            query = query.Where(p => p.BrandId == filter.BrandId.Value);
        }

        if (filter.SkinTypeId.HasValue)
        {
            query = query.Where(p => p.ProductSkinTypes.Any(pt => pt.SkinTypeId == filter.SkinTypeId.Value));
        }

        if (filter.IngredientId.HasValue)
        {
            query = query.Where(p => p.ProductIngredients.Any(pi => pi.IngredientId == filter.IngredientId.Value));
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        var totalCount = await query.CountAsync();

        // --- sorting ---
        query = (filter.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id) // "newest" default
        };

        // --- page ---
        var pageItems = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                BrandName = p.Brand.Name,
                CategoryName = p.Category.Name,
                MainImageUrl = p.Images.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault(),
                p.Volume,
                p.SPF,
                p.StockQuantity
            })
            .ToListAsync();

        var ratings = await GetRatingsAsync(pageItems.Select(p => p.Id));

        var items = pageItems.Select(p =>
        {
            ratings.TryGetValue(p.Id, out var rating);
            return new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                BrandName = p.BrandName,
                CategoryName = p.CategoryName,
                MainImageUrl = p.MainImageUrl,
                Volume = p.Volume,
                SPF = p.SPF,
                AverageRating = rating.Average,
                ReviewCount = rating.Count,
                InStock = p.StockQuantity > 0
            };
        }).ToList();

        return new PagedResultDto<ProductListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(int id)
    {
        var product = await _db.Products.AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Seller).ThenInclude(u => u.SellerProfile)
            .Include(p => p.Images.OrderBy(i => i.Id))
            .Include(p => p.ProductSkinTypes).ThenInclude(pt => pt.SkinType)
            .Include(p => p.ProductIngredients).ThenInclude(pi => pi.Ingredient)
            .AsSplitQuery() // several collection includes - avoid one giant cartesian row
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return null;
        }

        return await MapToDetailAsync(product);
    }

    // ------------------------------------------------------------------
    // Seller product management
    // ------------------------------------------------------------------

    public async Task<IList<ProductResponseDto>> GetSellerProductsAsync(string sellerId)
    {
        var products = await _db.Products.AsNoTracking()
            .Where(p => p.SellerId == sellerId)
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Seller).ThenInclude(u => u.SellerProfile)
            .Include(p => p.Images.OrderBy(i => i.Id))
            .Include(p => p.ProductSkinTypes).ThenInclude(pt => pt.SkinType)
            .Include(p => p.ProductIngredients).ThenInclude(pi => pi.Ingredient)
            .AsSplitQuery()
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

        var result = new List<ProductResponseDto>(products.Count);
        foreach (var product in products)
        {
            result.Add(await MapToDetailAsync(product));
        }

        return result;
    }

    public async Task<ProductResponseDto> CreateProductAsync(string sellerId, ProductCreateDto dto)
    {
        await EnsureApprovedSellerAsync(sellerId);
        await EnsureCatalogReferencesAsync(dto.BrandId, dto.CategoryId, dto.SkinTypeIds, dto.IngredientIds);

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            BrandId = dto.BrandId,
            CategoryId = dto.CategoryId,
            SellerId = sellerId,
            Volume = dto.Volume.Trim(),
            SPF = dto.SPF,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Images are URL strings only (no file upload in v1).
        foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            product.Images.Add(new ProductImage { ImageUrl = url.Trim() });
        }

        foreach (var skinTypeId in dto.SkinTypeIds.Distinct())
        {
            product.ProductSkinTypes.Add(new ProductSkinType { SkinTypeId = skinTypeId });
        }

        foreach (var ingredientId in dto.IngredientIds.Distinct())
        {
            product.ProductIngredients.Add(new ProductIngredient { IngredientId = ingredientId });
        }

        _db.Products.Add(product);
        await SaveChangesAsync();

        return (await GetProductByIdAsync(product.Id))!;
    }

    public async Task<ProductResponseDto?> UpdateProductAsync(string sellerId, int productId, ProductUpdateDto dto)
    {
        // Tracked entity so RowVersion (optimistic concurrency) is enforced.
        var product = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.ProductSkinTypes)
            .Include(p => p.ProductIngredients)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null || product.SellerId != sellerId)
        {
            return null; // not found OR not yours - same 404 to avoid probing
        }

        await EnsureCatalogReferencesAsync(dto.BrandId, dto.CategoryId, dto.SkinTypeIds, dto.IngredientIds);

        product.Name = dto.Name.Trim();
        product.Description = dto.Description.Trim();
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.BrandId = dto.BrandId;
        product.CategoryId = dto.CategoryId;
        product.Volume = dto.Volume.Trim();
        product.SPF = dto.SPF;
        product.IsActive = dto.IsActive;

        // Replace images and junction rows with the submitted lists.
        product.Images.Clear();
        foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            product.Images.Add(new ProductImage { ImageUrl = url.Trim() });
        }

        product.ProductSkinTypes.Clear();
        foreach (var skinTypeId in dto.SkinTypeIds.Distinct())
        {
            product.ProductSkinTypes.Add(new ProductSkinType { SkinTypeId = skinTypeId });
        }

        product.ProductIngredients.Clear();
        foreach (var ingredientId in dto.IngredientIds.Distinct())
        {
            product.ProductIngredients.Add(new ProductIngredient { IngredientId = ingredientId });
        }

        await SaveChangesAsync();

        return await MapToDetailAsync(product);
    }

    public async Task<bool> DeactivateProductAsync(string sellerId, int productId)
        => await SetActiveAsync(sellerId, productId, false);

    public async Task<bool> ActivateProductAsync(string sellerId, int productId)
        => await SetActiveAsync(sellerId, productId, true);

    public async Task<bool> AdminDeleteProductAsync(int productId)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.ProductSkinTypes)
            .Include(p => p.ProductIngredients)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
        {
            return false;
        }

        // OrderItem has a Restrict FK to Product, so a product that was ever
        // purchased can never be hard-deleted. EF wraps all of this in one
        // transaction, so a failed delete rolls back the child removals too.
        product.Images.Clear();
        product.ProductSkinTypes.Clear();
        product.ProductIngredients.Clear();
        _db.Products.Remove(product);

        try
        {
            await SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false; // still referenced (order items)
        }
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private async Task<bool> SetActiveAsync(string sellerId, int productId, bool isActive)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null || product.SellerId != sellerId)
        {
            return false;
        }

        product.IsActive = isActive;
        await SaveChangesAsync();
        return true;
    }

    /// <summary>Only approved sellers may publish/edit products.</summary>
    private async Task EnsureApprovedSellerAsync(string sellerId)
    {
        var approved = await _db.SellerProfiles.AsNoTracking()
            .AnyAsync(s => s.UserId == sellerId && s.ApprovalStatus == ApprovalStatus.Approved);

        if (!approved)
        {
            throw new AppException("Only approved sellers can publish products.", 403);
        }
    }

    /// <summary>Validates referenced catalog rows so bad ids return 400, not a raw FK 500.</summary>
    private async Task EnsureCatalogReferencesAsync(int brandId, int categoryId, IList<int> skinTypeIds, IList<int> ingredientIds)
    {
        if (!await _db.Brands.AsNoTracking().AnyAsync(b => b.Id == brandId))
        {
            throw new AppException($"Brand with id {brandId} does not exist.", 404);
        }

        if (!await _db.Categories.AsNoTracking().AnyAsync(c => c.Id == categoryId))
        {
            throw new AppException($"Category with id {categoryId} does not exist.", 404);
        }

        var skinTypeList = skinTypeIds.Distinct().ToList();
        var foundSkinTypes = await _db.SkinTypes.AsNoTracking()
            .CountAsync(s => skinTypeList.Contains(s.Id));

        if (foundSkinTypes != skinTypeList.Count)
        {
            throw new AppException("One or more skin types do not exist.", 404);
        }

        var ingredientList = ingredientIds.Distinct().ToList();
        var foundIngredients = await _db.Ingredients.AsNoTracking()
            .CountAsync(i => ingredientList.Contains(i.Id));

        if (foundIngredients != ingredientList.Count)
        {
            throw new AppException("One or more ingredients do not exist.", 404);
        }
    }

    /// <summary>Average rating + review count per product id (batched query).</summary>
    private async Task<Dictionary<int, (double Average, int Count)>> GetRatingsAsync(IEnumerable<int> productIds)
    {
        var ids = productIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<int, (double, int)>();
        }

        var rows = await _db.Reviews.AsNoTracking()
            .Where(r => ids.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Average = g.Average(r => (double)r.Rating),
                Count = g.Count()
            })
            .ToListAsync();

        return rows.ToDictionary(r => r.ProductId, r => (r.Average, r.Count));
    }

    private async Task<ProductResponseDto> MapToDetailAsync(Product product)
    {
        var ratings = await GetRatingsAsync(new[] { product.Id });
        ratings.TryGetValue(product.Id, out var rating);

        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name ?? string.Empty,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            SellerId = product.SellerId,
            SellerStoreName = product.Seller?.SellerProfile?.StoreName ?? string.Empty,
            Volume = product.Volume,
            SPF = product.SPF,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            Images = product.Images.OrderBy(i => i.Id).Select(i => i.ImageUrl).ToList(),
            SkinTypes = product.ProductSkinTypes.Select(pt => pt.SkinType.Name).ToList(),
            Ingredients = product.ProductIngredients.Select(pi => pi.Ingredient.Name).ToList(),
            AverageRating = rating.Average,
            ReviewCount = rating.Count
        };
    }

    /// <summary>
    /// Saves changes and converts a concurrency conflict (RowVersion mismatch:
    /// someone else edited the same product between our read and write) into a
    /// clean 409 instead of a 500.
    /// </summary>
    private async Task SaveChangesAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException(
                "This product was modified by someone else at the same time. Please refresh and try again.",
                409);
        }
    }
}
