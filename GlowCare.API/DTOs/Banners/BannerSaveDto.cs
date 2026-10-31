using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Banners;

/// <summary>Request to create or update a banner (Admin only).</summary>
public class BannerSaveDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Link { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
