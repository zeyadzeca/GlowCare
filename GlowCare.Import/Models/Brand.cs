using System.ComponentModel.DataAnnotations;

namespace GlowCare.Import.Models.Catalog;

/// <summary>A skincare brand (CeraVe, The Ordinary, ...). No BrandType field by design.</summary>
public class Brand
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Logo { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
