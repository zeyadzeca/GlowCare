using GlowCare.API.Models.Catalog;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Shopping;

/// <summary>
/// One line in a cart: a product and how many the customer wants.
/// Stock is validated against Product.StockQuantity at checkout time.
/// </summary>
public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    // Navigation properties
    public Cart Cart { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
