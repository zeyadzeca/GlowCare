using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Categories;

/// <summary>Request to update a category (Admin only).</summary>
public class CategoryUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
}
