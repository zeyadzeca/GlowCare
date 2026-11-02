using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Products;

/// <summary>
/// Request to create a product (approved sellers only).
/// Images are URL strings; skin types and ingredients are id lists.
/// </summary>
public class ProductCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Required]
    public int BrandId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Volume { get; set; } = string.Empty;

    [Range(0, 100)]
    public int? SPF { get; set; }

    public bool? IsActive { get; set; }

    public string? SellerId { get; set; }

    [MaxLength(10)]
    public IList<string> ImageUrls { get; set; } = new List<string>();

    public IList<int> SkinTypeIds { get; set; } = new List<int>();

    public IList<int> IngredientIds { get; set; } = new List<int>();
}
