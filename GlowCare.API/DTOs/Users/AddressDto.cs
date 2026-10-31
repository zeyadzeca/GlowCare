namespace GlowCare.API.DTOs.Users;

/// <summary>A shipping address owned by the authenticated user.</summary>
public class AddressDto
{
    public int Id { get; set; }

    public string City { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string BuildingNumber { get; set; } = string.Empty;
}
