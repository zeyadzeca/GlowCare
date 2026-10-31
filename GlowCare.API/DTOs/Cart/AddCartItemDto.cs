using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Cart;

/// <summary>Request to add a product to the cart (or increase its quantity).</summary>
public class AddCartItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;
}
