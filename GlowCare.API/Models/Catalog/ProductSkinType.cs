namespace GlowCare.API.Models.Catalog;

/// <summary>
/// Junction table: Product &lt;-&gt; SkinType (many-to-many).
/// Composite primary key (ProductId, SkinTypeId) prevents duplicate links.
/// </summary>
public class ProductSkinType
{
    public int ProductId { get; set; }

    public int SkinTypeId { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;

    public SkinType SkinType { get; set; } = null!;
}
