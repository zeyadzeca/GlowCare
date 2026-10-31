namespace GlowCare.API.DTOs.Products;

/// <summary>Full product details for the product details page.</summary>
public class ProductResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool InStock => StockQuantity > 0;

    public int BrandId { get; set; }

    public string BrandName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string SellerId { get; set; } = string.Empty;

    public string SellerStoreName { get; set; } = string.Empty;

    public string Volume { get; set; } = string.Empty;

    public int? SPF { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public IList<string> Images { get; set; } = new List<string>();

    public IList<string> SkinTypes { get; set; } = new List<string>();

    public IList<string> Ingredients { get; set; } = new List<string>();

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }
}
