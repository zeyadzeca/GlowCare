namespace GlowCare.API.DTOs.Cart;

/// <summary>The full cart of the authenticated user, with the calculated subtotal.</summary>
public class CartDto
{
    public int Id { get; set; }

    public IList<CartItemDto> Items { get; set; } = new List<CartItemDto>();

    public decimal Subtotal { get; set; }

    public int TotalItems { get; set; }
}
