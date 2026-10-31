using GlowCare.API.Models.Catalog;

namespace GlowCare.API.Models.Wishlist;

/// <summary>
/// One saved product in a wishlist.
/// The composite unique index on (WishlistId, ProductId) prevents duplicates.
/// </summary>
public class WishlistItem
{
    public int Id { get; set; }

    public int WishlistId { get; set; }

    public int ProductId { get; set; }

    // Navigation properties
    public Wishlist Wishlist { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
