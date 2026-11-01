namespace GlowCare.Import.Models.Catalog;

/// <summary>
/// Junction table: Product &lt;-&gt; Ingredient (many-to-many).
/// Composite primary key (ProductId, IngredientId) prevents duplicate links.
/// </summary>
public class ProductIngredient
{
    public int ProductId { get; set; }

    public int IngredientId { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;

    public Ingredient Ingredient { get; set; } = null!;
}
