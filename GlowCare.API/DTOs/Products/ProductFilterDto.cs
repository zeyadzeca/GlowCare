using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Products;

/// <summary>
/// All supported catalog filters in one request. Every filter is optional and
/// they combine with AND, e.g.:
/// GET /api/products?brandId=1&amp;skinTypeId=2&amp;minPrice=100&amp;maxPrice=500&amp;page=1&amp;pageSize=10
/// </summary>
public class ProductFilterDto
{
    /// <summary>Free-text search on product name.</summary>
    [MaxLength(200)]
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int? SkinTypeId { get; set; }

    public int? IngredientId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }

    /// <summary>"price_asc" | "price_desc" | "newest" | "name". Default: newest.</summary>
    [MaxLength(20)]
    public string? SortBy { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
