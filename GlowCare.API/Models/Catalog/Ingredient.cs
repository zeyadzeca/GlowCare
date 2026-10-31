using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Catalog;

/// <summary>A skincare ingredient (e.g. Hyaluronic Acid) used for product filtering.</summary>
public class Ingredient
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Navigation property
    public ICollection<ProductIngredient> ProductIngredients { get; set; } = new List<ProductIngredient>();
}
