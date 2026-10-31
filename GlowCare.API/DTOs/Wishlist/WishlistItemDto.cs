namespace GlowCare.API.DTOs.Wishlist;

/// <summary>One saved product in the wishlist.</summary>
public class WishlistItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string BrandName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? MainImageUrl { get; set; }
}
