using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Brands;

/// <summary>Request to create a brand (Admin only).</summary>
public class BrandCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Logo { get; set; } = string.Empty;
}
