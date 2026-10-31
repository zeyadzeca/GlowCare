namespace GlowCare.API.DTOs.Wishlist;

/// <summary>The full wishlist of the authenticated user.</summary>
public class WishlistDto
{
    public int Id { get; set; }

    public IList<WishlistItemDto> Items { get; set; } = new List<WishlistItemDto>();
}
