using GlowCare.API.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Shopping;

/// <summary>
/// A persistent shopping cart. One per registered user (unique UserId).
/// There is no guest cart and no SessionId by design.
/// </summary>
public class Cart
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
