using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Cart;

/// <summary>Request to set a new quantity for an existing cart item.</summary>
public class UpdateCartItemDto
{
    [Range(1, 1000)]
    public int Quantity { get; set; }
}
