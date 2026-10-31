using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Content;

/// <summary>
/// A promotional banner shown on the home page.
/// Admin manages these; only IsActive = true banners are shown publicly.
/// </summary>
public class Banner
{
    public int Id { get; set; }

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
