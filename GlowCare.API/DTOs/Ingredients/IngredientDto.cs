namespace GlowCare.API.DTOs.Ingredients;

/// <summary>A skincare ingredient used for product filtering.</summary>
public class IngredientDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int ProductCount { get; set; }
}
