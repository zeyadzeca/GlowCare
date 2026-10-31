namespace GlowCare.API.DTOs.Brands;

/// <summary>A brand as returned by the API.</summary>
public class BrandDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Logo { get; set; } = string.Empty;

    public int ProductCount { get; set; }
}
