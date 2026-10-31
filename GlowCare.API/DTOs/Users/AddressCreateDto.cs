using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Users;

/// <summary>Request to add a new address for the authenticated user.</summary>
public class AddressCreateDto
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
