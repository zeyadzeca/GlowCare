using System.ComponentModel.DataAnnotations;

namespace GlowCare.Import.Models.Catalog;

/// <summary>
/// A skin type used to filter products. Seeded with the 5 standard values:
/// Oily, Dry, Combination, Sensitive, Normal.
/// </summary>
public class SkinType
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public ICollection<ProductSkinType> ProductSkinTypes { get; set; } = new List<ProductSkinType>();
}
