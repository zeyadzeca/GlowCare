namespace GlowCare.API.DTOs.Products;

/// <summary>Lightweight product card used in grids and carousels.</summary>
public class ProductListDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string BrandName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string? MainImageUrl { get; set; }

    public string? Volume { get; set; }

    public int? SPF { get; set; }

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public bool InStock { get; set; }
}
