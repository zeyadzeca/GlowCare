using GlowCare.API.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Wishlist;

/// <summary>
/// A customer's wishlist. One per registered user (unique UserId).
/// </summary>
public class Wishlist
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;

    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}
