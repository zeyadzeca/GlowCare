using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Wishlist;

/// <summary>Request to save a product to the wishlist.</summary>
public class AddWishlistItemDto
{
    [Required]
    public int ProductId { get; set; }
}
