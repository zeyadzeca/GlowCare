using System.ComponentModel.DataAnnotations;

namespace GlowCare.Import.Models.Catalog;

/// <summary>One image of a product. Stored as a URL string (no file upload in v1).</summary>
public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation property
    public Product Product { get; set; } = null!;
}
