namespace GlowCare.API.DTOs.Cart;

/// <summary>One line in the cart, with live product info and the line total.</summary>
public class CartItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string BrandName { get; set; } = string.Empty;

    public string? MainImageUrl { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>Current stock, so the UI can cap the quantity selector.</summary>
    public int AvailableStock { get; set; }
}
