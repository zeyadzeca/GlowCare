using GlowCare.API.Models.User;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GlowCare.API.Models.Identity;

/// <summary>
/// The single user table for all account types (Admin, Seller, Customer).
/// Roles are managed by Identity through AspNetUserRoles - no role column here.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    public SellerProfile? SellerProfile { get; set; }
}
