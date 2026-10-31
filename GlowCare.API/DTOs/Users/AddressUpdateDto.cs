using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Users;

/// <summary>Request to update an existing address. Same rules as creation.</summary>
public class AddressUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string BuildingNumber { get; set; } = string.Empty;
}
