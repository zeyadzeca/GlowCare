using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Ingredients;

/// <summary>Request to create or update an ingredient (Admin only).</summary>
public class IngredientSaveDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
}
