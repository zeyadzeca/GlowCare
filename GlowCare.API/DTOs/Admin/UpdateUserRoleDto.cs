using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.DTOs.Admin;

/// <summary>Request to change a user's role (Admin only). Valid roles: Admin, Seller, Customer.</summary>
public class UpdateUserRoleDto
{
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = string.Empty;

    /// <summary>true = grant the role, false = remove it.</summary>
    [Required]
    public bool Assign { get; set; }
}
