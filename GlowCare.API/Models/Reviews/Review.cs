using GlowCare.API.Models.Catalog;
using GlowCare.API.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Reviews;

/// <summary>
/// A customer's review of a product.
/// - Rating must be 1..5 (enforced by a database check constraint).
/// - One review per customer per product (unique index on UserId + ProductId).
/// - Business rule (enforced in the service layer): only customers whose
///   delivered/paid order contained this product may review it.
/// </summary>
public class Review
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
