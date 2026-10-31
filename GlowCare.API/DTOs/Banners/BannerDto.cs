namespace GlowCare.API.DTOs.Banners;

/// <summary>A home-page promotional banner.</summary>
public class BannerDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
