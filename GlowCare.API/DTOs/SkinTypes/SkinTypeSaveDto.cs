using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.SkinTypes;

/// <summary>Request to create a skin type (Admin only). Skins types have no description in the schema.</summary>
public class SkinTypeSaveDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}
