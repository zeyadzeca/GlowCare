using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.User;

public class UserPhoneNumberDto
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Request to add a new phone number for the authenticated user.</summary>
public class CreatePhoneNumberDto
{
    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}

/// <summary>Request to update an existing phone number. Same rules as creation.</summary>
public class UpdatePhoneNumberDto
{
    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}
