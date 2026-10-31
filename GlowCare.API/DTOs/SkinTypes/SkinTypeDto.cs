namespace GlowCare.API.DTOs.SkinTypes;

/// <summary>A skin type used for product filtering (Id + Name only, per the agreed schema).</summary>
public class SkinTypeDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
