using System.ComponentModel.DataAnnotations;
using GlowCare.API.Models.Identity;

namespace GlowCare.API.Models.Catalog;

/// <summary>
/// A product published by an approved seller.
/// Price/Stock are validated by check constraints in AppDbContext.
/// RowVersion (approved schema addition) prevents overselling:
/// if two checkouts update the same product concurrently, the second one fails.
/// </summary>
public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int BrandId { get; set; }

    public int CategoryId { get; set; }

    /// <summary>The ApplicationUser Id of the owning seller (ASP.NET Identity user).</summary>
    [Required]
    [MaxLength(450)]
    public string SellerId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Volume { get; set; } = string.Empty; // e.g. "50ml"

    /// <summary>Sun protection factor; null for products that have none.</summary>
    public int? SPF { get; set; }

    /// <summary>Soft delete flag. Sellers deactivate instead of hard-deleting.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optimistic concurrency token managed by SQL Server.</summary>
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Brand Brand { get; set; } = null!;

    public Category Category { get; set; } = null!;

    public ApplicationUser Seller { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    public ICollection<ProductSkinType> ProductSkinTypes { get; set; } = new List<ProductSkinType>();

    public ICollection<ProductIngredient> ProductIngredients { get; set; } = new List<ProductIngredient>();
}
