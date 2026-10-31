using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Categories;

/// <summary>A product category as returned by the API.</summary>
public class CategoryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int ProductCount { get; set; }
}
