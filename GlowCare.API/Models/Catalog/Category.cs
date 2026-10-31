using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Catalog;

/// <summary>
/// A product type (Cleansers, Moisturizers, Sunscreens, Serums, Toners).
/// No subcategories - flat list by design.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
